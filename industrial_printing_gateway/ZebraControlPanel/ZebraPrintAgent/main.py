from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from printer import print_zpl_usb, list_windows_printers, get_printers_info, lista_puertos_com_usb_vid_pid
from typing import List, Dict, Any
import database
import json
app = FastAPI()

origins = ["*"]

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.on_event("startup")
def startup_event():
    database.init_db()

class PrinterInfoRequest(BaseModel):
    level_info: int

class PrinterInfoResponse(BaseModel):
    name: str
    info: dict
    conn: dict | None
    status: bool = True

class ZPLRequest(BaseModel):
    printer_name: str
    zpl_code: List[str]

class PrinterConfigRequest(BaseModel):
    name: str
    status: bool = True
    port: str = None
    vendor_id: str = None
    product_id: str = None
    alias: str = None
    debug_mode: bool = False

class PrinterConfigUpdateRequest(BaseModel):
    id: int
    status: bool = True
    port: str = None
    vendor_id: str = None
    product_id: str = None
    alias: str = None
    debug_mode: bool = False

@app.post("/list-host-printers/")
async def list_printers(request: PrinterInfoRequest):
    printer_info_list = []
    
    printers = list_windows_printers()
    printer_names = [p[2] for p in printers]
    
    printer_info_list : PrinterInfoResponse = await get_printers_info(printer_names, request.level_info)

    return {"printers": printer_info_list}

@app.post("/save-printer-config/")
def save_printer_config(request: PrinterConfigRequest):
    try:
        database.save_printer_config(
            name=request.name, 
            status=request.status,
            port=request.port,
            vendor_id=request.vendor_id,
            product_id=request.product_id,
            alias=request.alias,
            debug_mode=request.debug_mode
        )
        return {"status": "success", "message": f"Configuration for {request.name} saved."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@app.post("/update-printer-config/")
def update_printer_config(request: PrinterConfigUpdateRequest):
    try:
        database.update_printer_config(
            id=request.id, 
            status=request.status,
            port=request.port,
            vendor_id=request.vendor_id,
            product_id=request.product_id,
            alias=request.alias,
            debug_mode=request.debug_mode
        )
        return {"status": "success", "message": f"Configuration for {request.id} updated."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.delete("/delete-printer/{printer_id}")
def delete_printer(printer_id: int):
    try:
        success = database.delete_printer(printer_id)
        if success:
            return {"status": "success", "message": f"Printer {printer_id} deleted."}
        else:
            raise HTTPException(status_code=404, detail="Printer not found.")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/stored-printers/")
def get_stored_printers():
    try:
        printers = database.get_all_printers()
        return {"printers": printers}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/print-zpl/")
def print_zpl(request: ZPLRequest):
    try:
        print_zpl_usb(request.printer_name, request.zpl_code)
        return {"status": "success"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@app.get("/health/")
def health_check():
    return {"status": "healthy"}

@app.get("/get-ip/")
def get_ip():
    import socket
    hostname = socket.gethostname()
    ip_address = socket.gethostbyname(hostname)
    port = 8090
    return {"hostname": hostname, "ip_address": ip_address, "port": port}

@app.post("/get-printer-status/{printer_id}")
async def getPrinterStatus(printer_id: int):
    try:
        printer = database.get_printer_by_id(printer_id)
        connected_printers = await lista_puertos_com_usb_vid_pid()

        for connected_printer in connected_printers:
            if(connected_printer["vid"] == printer["vendor_id"] and connected_printer["pid"] == printer["product_id"]):
                return {"status": True}

        return {"status": False }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))