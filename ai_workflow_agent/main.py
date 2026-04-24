import asyncio
import logging
import os
from whatsapp_control.clear_history import start_background_cleaner, background_task
import models.models as ModelsJson
import bot_control.bot_controller as bot_controller
import faiss_control.file_synchronization as syncfiles
from http.client import HTTPException
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from datetime import datetime
from fastapi import FastAPI, Query, Request, Response
from pathlib import Path
from whatsapp_control.whatsapp_controller import send_message_to_user
from contextlib import asynccontextmanager
from dotenv import load_dotenv
from db_control.db_controller import setup_checkpoint_db
load_dotenv()

log_level = os.getenv("LOG_LEVEL", "INFO").upper()

logging.basicConfig(
    level=getattr(logging, log_level),
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)

logger = logging.getLogger(__name__)


async def check_and_sync_files():
    try:
        tmp_dir = Path(os.getenv("FAISS_FOLDER_PATH"))
        print("Check folder...")
        if not tmp_dir.exists():
            print("The folder does not exist...")
            os.makedirs(tmp_dir)
            print("'tmp' directory created, syncing files from S3...")
            sync_files_in_s3()
        if tmp_dir.exists():
            print("Synchronizing files from S3...")
            sync_files_in_s3()
    except Exception as e:
        logging.error(f"An error occurred: {e}")

@asynccontextmanager
async def lifespan(app: FastAPI):
    print("Setup checkpoint tables on database...")
    await setup_checkpoint_db()
    await check_and_sync_files()
    print("Starting background cleaner...")
    await start_background_cleaner()
    yield
    global background_task
    if background_task:
        background_task.cancel()
        try:
            await background_task
        except asyncio.CancelledError:
            logger.error("Cleanup of inactive threads in bot memory stopped successfully.")
    
app = FastAPI(lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/syncfiles")
def sync_files_in_s3():
    try:
        syncfiles.download_file()
        return {'message': "File synchronization from S3 to local completed successfully"}
    except Exception as e:
        logging.error(f"An error occurred in the synchronization: {e}")
        return {f"Unexpected error: {e}"}

@app.post("/chatbot")
def test(get_request: ModelsJson.botModel):
    now = datetime.now()
    formatted_now = now.strftime("%d-%m-%Y %H:%M:%S")
    response = bot_controller.agent_request(get_request.request, get_request.thread_id)
    return {'response': response, 'datetime': formatted_now}

@app.get("/whatsapp")
async def verify_webhook(
    hub_mode: str = Query(alias="hub.mode"),
    hub_verify_token: str = Query(alias="hub.verify_token"),
    hub_challenge: str = Query(alias="hub.challenge")
):
    if hub_mode == "subscribe" and hub_verify_token == os.getenv("VERIFICATION_TOKEN"):
        return Response(content=hub_challenge, media_type="text/plain")
    raise HTTPException(status_code=403, detail="Verification failed")
processed_messages = set()

@app.post("/whatsapp")
async def handle_webhook(request: Request):
    try:
        data = await request.json()
        if data.get("object") == "whatsapp_business_account":
            for entry in data.get("entry", []):
                for change in entry.get("changes", []):
                    messages = change.get("value", {}).get("messages", [])
                    for message in messages:
                        from_id = message.get("from") 
                        message_body = message.get("text", {}).get("body")
                        message_id = message.get("id") 

                        if message_id in processed_messages:
                            print(f"Mensaje duplicado detectado. ID del mensaje: {message_id}")
                            continue  
                        processed_messages.add(message_id)
                        
                        print(f"Mensaje recibido de {from_id}: {message_body}")
                        message_response = bot_controller.agent_request(message_body, from_id)
                        send_message_to_user(from_id, message_response.get('resp_bot'))

        return JSONResponse(content={"status": "success"}, status_code=200)
    except Exception as e:
        print(f"Error al procesar el mensaje: {e}")
        return JSONResponse(content={"error": str(e)}, status_code=500)