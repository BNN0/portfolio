from datetime import datetime, timedelta
import asyncio
import whatsapp_control.whatsapp_controller as whc
import os
import bot_control.bot_controller as bot_controller
from dotenv import load_dotenv
load_dotenv()

thread_activity = {}
background_task = None
delete_time = int(os.getenv("INACTIVE_THREAD_TIMEOUT", 5))
INACTIVITY_THRESHOLD = timedelta(minutes=delete_time)
    
async def start_background_cleaner():
    global background_task
    if background_task is None or background_task.done():
        background_task = asyncio.create_task(clean_inactive_threads())
        print("Cleanup of inactive threads started in the background.")
    
async def clean_inactive_threads():
    while True:
        try:
            print("Checking for inactive threads...")
            current_time = datetime.now()
            inactive_threads = [
                thread_id for thread_id, last_activity in thread_activity.items()
                if current_time - last_activity > INACTIVITY_THRESHOLD
            ]
            for thread_id in inactive_threads:
                config = {"configurable": {"thread_id": thread_id}}
                messages_thread_id = await bot_controller.get_messages_for_threadIds_from_memory(config)
                
                if messages_thread_id and len(messages_thread_id) > 1:
                    # bot_controller.graph.update_state(config, {"messages": [bot_controller.RemoveMessage(id=m.id) for m in messages_thread_id]})
                    
                    await bot_controller.remove_messages_from_memory(config, messages_thread_id)

                    del thread_activity[thread_id]

                    notification_message = (
                        "Parece que el tiempo de espera se ha agotado y no hemos recibido ninguna consulta. "
                        "Si necesitas ayuda más tarde, no dudes en volver a contactarnos."
                    )
                    
                    whc.send_message_to_user(thread_id, notification_message)
                    
            await asyncio.sleep(20)
        except Exception as e:
            print(f"Error en el limpiador de hilos inactivos: {e}")
            await asyncio.sleep(40)
            
def update_thread_activity(thread_id):
    print(f"Updating thread activity for {thread_id}")
    thread_activity[thread_id] = datetime.now()
            
            

