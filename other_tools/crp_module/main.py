from fastapi import FastAPI, File, UploadFile, Form, BackgroundTasks, Request
from datetime import datetime
from fastapi.responses import HTMLResponse
from fastapi.staticfiles import StaticFiles
import os
import shutil
from pydantic import BaseModel, Field, field_validator
from typing import Optional, List
from src.db_manager import (
    get_parts_list, get_part_details, upsert_inventory_data, 
    get_daily_status, get_part_status_history, recalculate_inventory_status,
    get_projected_inventory, find_out_of_stock_risk, update_stock_limit
)
from src.db_setup.extractor import run_data_import

app = FastAPI()

# In-memory status tracking for import
IMPORT_STATUS = {"status": "idle", "message": "", "progress": 0}

def background_import(file_path, initial_date):
    global IMPORT_STATUS
    try:
        IMPORT_STATUS["status"] = "processing"
        IMPORT_STATUS["message"] = "Initializing..."
        
        def update_msg(msg):
            IMPORT_STATUS["message"] = msg
            
        success = run_data_import(file_path, initial_date, status_callback=update_msg)
        if success:
            IMPORT_STATUS["status"] = "success"
            IMPORT_STATUS["message"] = "Import completed successfully!"
        else:
            IMPORT_STATUS["status"] = "error"
            IMPORT_STATUS["message"] = "Import failed during processing."
    except Exception as e:
        import traceback
        traceback.print_exc()
        IMPORT_STATUS["status"] = "error"
        IMPORT_STATUS["message"] = f"Error: {str(e)}"
    finally:
        # Cleanup
        if os.path.exists(file_path):
            try:
                os.remove(file_path)
            except:
                pass

base_dir = os.path.dirname(os.path.abspath(__file__))
src_path = os.path.join(base_dir, "src")
app.mount("/src", StaticFiles(directory=src_path), name="src")

utils_path = os.path.join(base_dir, "src/pages/utils")
app.mount("/utils", StaticFiles(directory=utils_path), name="utils")

@app.get("/", response_class=HTMLResponse)
async def read_index():
    # Get the directory where this script is located
    base_dir = os.path.dirname(os.path.abspath(__file__))
    file_path = os.path.join(base_dir, "src/pages/index.html")
    
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            html_content = f.read()
        return HTMLResponse(content=html_content, status_code=200)
    except FileNotFoundError:
        return HTMLResponse(content="<h1>Error: code.html no encontrado</h1>", status_code=404)

@app.get("/inventory", response_class=HTMLResponse)
async def read_inventory():
    # Get the directory where this script is located
    base_dir = os.path.dirname(os.path.abspath(__file__))
    file_path = os.path.join(base_dir, "src/pages/inventory.html")
    
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            html_content = f.read()
        return HTMLResponse(content=html_content, status_code=200)
    except FileNotFoundError:
        return HTMLResponse(content="<h1>Error: code.html no encontrado</h1>", status_code=404)


class OrderLogisticsRequest(BaseModel):
    partno_id: int
    process_type_name: str
    entry_date: str
    quantity: float
    invoice_id: Optional[int] = 0

    @classmethod
    @field_validator('process_type_name')
    def validate_process_type(cls, v):
        allowed = {"REQUIRED QUANTITY", "INCOMING DELIVERY", "ON THE WAY", "STATUS QUANTITY"}
        if v not in allowed:
            raise ValueError(f"process_type_name debe ser uno de: {', '.join(allowed)}")
        return v

class PartSearchRequest(BaseModel):
    query: Optional[str] = ""
    part_id: Optional[int] = None
    entry_date: Optional[str] = None

class PartLimitUpdateRequest(BaseModel):
    part_id: int
    stock_limit: int

# API endpoints

@app.post("/api/inventory/requestedqty")
async def set_requested_qty(request: OrderLogisticsRequest):
    try:
        success = upsert_inventory_data(
            request.partno_id, 
            "REQUIRED QUANTITY", 
            request.quantity, 
            request.entry_date
        )
        return {"status": "success" if success else "error"}
    except Exception as e:
        return {"error": str(e)}
    
@app.post("/api/inventory/incomingdelivery")
async def set_incoming_delivery(request: OrderLogisticsRequest):
    try:
        success = upsert_inventory_data(
            request.partno_id, 
            "INCOMING DELIVERY", 
            request.quantity, 
            request.entry_date
        )
        return {"status": "success" if success else "error"}
    except Exception as e:
        return {"error": str(e)}
    
@app.post("/api/inventory/ontheway")
async def set_on_the_way(request: OrderLogisticsRequest):
    try:
        success = upsert_inventory_data(
            request.partno_id, 
            "ON THE WAY", 
            request.quantity, 
            request.entry_date
        )
        return {"status": "success" if success else "error"}
    except Exception as e:
        return {"error": str(e)}
    
@app.post("/api/inventory/updateinventorystatus")
async def set_update_inventory_status(request: OrderLogisticsRequest):
    try:
        success = upsert_inventory_data(
            request.partno_id, 
            "STATUS QUANTITY", 
            request.quantity, 
            request.entry_date
        )
        return {"status": "success" if success else "error"}
    except Exception as e:
        return {"error": str(e)}
    
@app.post("/api/inventory/partno/get/list")
async def api_get_part_no_list(request: PartSearchRequest):
    parts = get_parts_list(request.query)
    return {"parts": parts}

@app.post("/api/inventory/partno/get/info")
async def api_get_part_no_info(request: PartSearchRequest):
    if not request.part_id:
        return {"error": "part_id is required"}
    part = get_part_details(request.part_id)
    return {"part": part}

@app.post("/api/inventory/get/daily-status")
async def api_get_daily_status(request: PartSearchRequest):
    if not request.part_id:
        return {"error": "part_id is required"}
    
    # Use provided entry_date or default to today
    today_str = datetime.now().strftime("%Y-%m-%d")
    target_date = request.entry_date or today_str
    
    status = get_daily_status(request.part_id, target_date)
    
    # Fallback / Projection Logic for STATUS QUANTITY
    if target_date > today_str:
        # Future Projection
        projection = get_projected_inventory(request.part_id, target_date)
        if projection is not None:
            if not status: status = {}
            status["STATUS QUANTITY"] = projection
    elif not status or "STATUS QUANTITY" not in status:
        # Today's Fallback (current total)
        part_info = get_part_details(request.part_id)
        if part_info and 'total' in part_info:
            if not status: status = {}
            status["STATUS QUANTITY"] = part_info['total']
            
    return {"status": status}


@app.post("/api/inventory/get/all-status")
async def api_get_all_status(request: PartSearchRequest):
    if not request.part_id:
        return {"error": "part_id is required"}
    history = get_part_status_history(request.part_id)
    risk = find_out_of_stock_risk(request.part_id)
    return {"history": history, "risk": risk}
@app.post("/api/inventory/recalculate")
async def api_recalculate_inventory(request: PartSearchRequest):
    if not request.part_id:
        return {"error": "part_id is required"}
    
    success = recalculate_inventory_status(request.part_id)
    return {"status": "success" if success else "error"}

@app.post("/api/inventory/partno/update/limit")
async def api_update_stock_limit(request: PartLimitUpdateRequest):
    if not request.part_id:
        return {"error": "part_id is required"}
    success = update_stock_limit(request.part_id, request.stock_limit)
    if success:
        return {"success": True, "message": "Stock limit updated."}
    else:
        return {"success": False, "message": "Failed to update."}
@app.post("/api/inventory/upload")
async def api_upload_inventory(background_tasks: BackgroundTasks, file: UploadFile = File(...), initial_date: str = Form(...)):
    global IMPORT_STATUS
    
    if IMPORT_STATUS["status"] == "processing":
        return {"status": "error", "message": "An import is already in progress."}
        
    temp_path = f"temp_{file.filename}"
    try:
        # Save temp file
        with open(temp_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)
        
        # Reset status
        IMPORT_STATUS["status"] = "processing"
        IMPORT_STATUS["message"] = "File uploaded, starting background process..."
        
        # Start background task
        background_tasks.add_task(background_import, temp_path, initial_date)
        
        return {"status": "started"}
    except Exception as e:
        if os.path.exists(temp_path):
            os.remove(temp_path)
        return {"status": "error", "message": str(e)}

@app.get("/api/inventory/upload-status")
async def get_upload_status():
    return IMPORT_STATUS

@app.get("/api/dashboard/summary")
async def api_get_dashboard_summary():
    from src.db_manager import get_dashboard_summary
    summary = get_dashboard_summary()
    if summary is None:
        return {"error": "Failed to fetch dashboard summary"}
    return summary

@app.post("/api/inventory/acknowledge")
async def api_acknowledge_risk(request: Request):
    from src.db_manager import acknowledge_part_risk
    data = await request.json()
    part_id = data.get("part_id")
    until_date = data.get("until_date")
    
    if not part_id or not until_date:
        return {"error": "Missing part_id or until_date"}
        
    success = acknowledge_part_risk(part_id, until_date)
    return {"success": success}

