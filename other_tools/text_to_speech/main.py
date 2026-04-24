from fastapi import FastAPI, HTTPException
from fastapi.responses import HTMLResponse, FileResponse
from fastapi.staticfiles import StaticFiles
from pydantic import BaseModel
import os
import voice_generator

app = FastAPI()

# Mount the audio directory to serve generated files
if not os.path.exists("audio"):
    os.makedirs("audio")
app.mount("/audio", StaticFiles(directory="audio"), name="audio")

class SynthesisRequest(BaseModel):
    text: str
    language: str = "es"
    accent: str = "com.mx"

@app.get("/", response_class=HTMLResponse)
async def read_index():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    file_path = os.path.join(base_dir, "index.html")
    
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            html_content = f.read()
        return HTMLResponse(content=html_content, status_code=200)
    except FileNotFoundError:
        return HTMLResponse(content="<h1>Error: index.html no encontrado</h1>", status_code=404)

@app.post("/synthesize")
async def synthesize_voice(request: SynthesisRequest):
    if not request.text.strip():
        raise HTTPException(status_code=400, detail="Text cannot be empty")
    
    try:
        filename = await voice_generator.generate_speech(
            text=request.text,
            language=request.language,
            accent=request.accent
        )
        return {"filename": filename, "url": f"/audio/{filename}"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/history")
async def get_history():
    files = await voice_generator.list_audio_files()
    # Sort by filename (which includes timestamp) descending
    files.sort(reverse=True)
    return {"files": files}

@app.delete("/delete/{filename}")
async def delete_file(filename: str):
    success = await voice_generator.delete_audio_file(filename)
    if not success:
        raise HTTPException(status_code=404, detail="File not found")
    return {"message": "File deleted successfully"}

@app.get("/favicon.ico", include_in_schema=False)
async def favicon():
    return FileResponse("favicon.ico") if os.path.exists("favicon.ico") else HTMLResponse(status_code=204)
