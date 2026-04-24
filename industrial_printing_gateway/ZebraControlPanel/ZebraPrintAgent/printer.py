from typing import List
from pydantic import BaseModel
import usb.core
import usb.util
import usb.backend.libusb1
import usb.backend.openusb
from serial.tools import list_ports
import win32print
from win32com import client
import serial
import time
import pywinusb.hid as hid
import win32ui
import pywintypes
import win32con
import re
import json

import database


class PrinterConnDict(BaseModel):
    vid: str
    pid: str
    bus: int
    address: int 
    manufacturer: str | None
    product: str | None
    serial_number: str | None
    class_dev: int

class PrinterInfoResponse(BaseModel):
    name: str
    info: dict
    conn: PrinterConnDict | None

# Try to load libusb backend with explicit path and fallbacks
def get_usb_backend():
    # Primary path from user configuration
    primary_path = r"C:\libusb-1.0.29\MinGW64\dll\libusb-1.0.dll"
    
    try:
        backend = usb.backend.libusb1.get_backend(find_library=lambda x: primary_path)
        if backend:
            print(f"Successfully loaded USB backend from {primary_path}")
            return backend
    except Exception as e:
        print(f"Failed to load USB backend from {primary_path}: {e}")

    # Fallback to default search
    try:
        backend = usb.backend.libusb1.get_backend()
        if backend:
            print("Loaded USB backend from default system path")
            return backend
    except Exception as e:
        print(f"Failed to load default USB backend: {e}")

    # Try openusb as last resort
    try:
        backend = usb.backend.openusb.get_backend()
        if backend:
            print("Loaded OpenUSB backend")
            return backend
    except Exception as e:
        print(f"Failed to load OpenUSB backend: {e}")

    print("WARNING: No USB backend could be loaded. USB device discovery will likely fail.")
    return None

backend = get_usb_backend()


def clean_dict(obj):
    if isinstance(obj, dict):
        return {k: clean_dict(v) for k, v in obj.items()}
    elif isinstance(obj, (str, int, float, bool, type(None))):
        return obj
    elif isinstance(obj, (list, tuple)):
        return [clean_dict(i) for i in obj]
    else:
        return str(obj)

def safe_get_string(dev, index):
    try:
        if index:
            return usb.util.get_string(dev, index)
    except Exception:
        return None
    return None

async def lista_puertos_com_usb_vid_pid():
    try:
        ports_list = []
        wmi = client.GetObject("winmgmts:")
        for port in wmi.InstancesOf("Win32_SerialPort"):
            pnp_id = port.PNPDeviceID
            if "USB" in pnp_id:
                match = re.search(r'VID_([0-9A-F]+)&PID_([0-9A-F]+)', pnp_id, re.I)
                if match:
                    vid = match.group(1)
                    pid = match.group(2)

                    ports_list.append({"vid":vid, "pid":pid, "name": port.Name})
        
        for usb in wmi.InstancesOf("Win32_USBHub"):
            pnp_id = usb.DeviceID
            match = re.search(r'VID_([0-9A-F]+)&PID_([0-9A-F]+)', pnp_id, re.I)
            if match:
                vid = match.group(1)
                pid = match.group(2)

                ports_list.append({"vid":vid, "pid":pid, "name": usb.Name})

        return ports_list
    except Exception as e:
        print("Error list ports com usb vid pid: ", e)


# Obtener lista de impresoras en el host Windows
def list_windows_printers():
    printers = win32print.EnumPrinters(
        win32print.PRINTER_ENUM_LOCAL,
        None,
        1
    )
    return printers

#Enviar comando ZPL a impresora instalada en Windows
def print_zpl_usb(printer_name, zpl_code : List[any]):
    # Open the printer
    h_printer = win32print.OpenPrinter(printer_name)
    try:
        # Start a document and a page
        h_job = win32print.StartDocPrinter(h_printer, 1, ("Print Job", None, "RAW"))
        try:
            # Send the ZPL data
            if h_job > 0:
                win32print.StartPagePrinter(h_printer)
                # Data must be bytes
                for chunk in zpl_code:
                    win32print.WritePrinter(h_printer, chunk.encode())
                win32print.EndPagePrinter(h_printer)
        finally:
            win32print.EndDocPrinter(h_printer)
    finally:
        win32print.ClosePrinter(h_printer)


def is_usb_printer(dev):
    try:
          for cfg in dev:
            for intf in cfg:
                if intf.bInterfaceClass == 0x07:
                    return True
    except Exception:
        pass
    return False

def list_com_ports():
    ports = []
    for p in list_ports.comports():
        ports.append({
            "device": p.device,
            "vid": f"{p.vid:04X}" if p.vid else None,
            "pid": f"{p.pid:04X}" if p.pid else None,
            "serial_number": p.serial_number
        })

    return ports

def map_usb_to_com(usb_dev, com_ports):
    for port in com_ports:
        if (
            port["vid"] == f"{usb_dev.idVendor:04X}" and
            port["pid"] == f"{usb_dev.idProduct:04X}"
        ):
            return port["device"]

    return None

def match_printer_by_com_port(com_port):
    # Use the explicitly initialized backend to avoid "no backend available" errors
    try:
        devices = usb.core.find(find_all=True, backend=backend)
    except Exception as e:
        print(f"Error during usb.core.find: {e}")
        # Final fallback if the backend we found still fails for some reason
        try:
            devices = usb.core.find(find_all=True)
        except Exception:
            return None

    if not devices:
        return None

    for dev in devices:
        if is_usb_printer(dev):
            port = map_usb_to_com(dev, list_com_ports())
            if com_port == port:
                return {
                    "vid": f"{dev.idVendor:04X}",
                    "pid": f"{dev.idProduct:04X}",
                    "bus": dev.bus,
                    "address": dev.address,
                    "manufacturer": safe_get_string(dev, dev.iManufacturer),
                    "product": safe_get_string(dev, dev.iProduct),
                    "serial_number": safe_get_string(dev, dev.iSerialNumber),
                    "class_dev": dev.bDeviceClass,
                }
            
async def check_registered_printer(filtered_devices: list[PrinterInfoResponse]):
    try:
        for device in filtered_devices:
            print(device["name"])
            print(device["conn"]["vid"])
            print(device["conn"]["pid"])
            name : str = device["name"]
            vid : str = device["conn"]["vid"]
            pid : str = device["conn"]["pid"]
            print(database.get_printer_by_filter(name=name, vid=vid, pid=pid))

        return filtered_devices
    except Exception as e:
        print("Error filtering connected_devices: ", e)

async def check_registered_printer(name : str, conn : dict) -> bool:
    try:
        if(conn):
            vid : str = conn["vid"]
            pid : str = conn["pid"]

            if(database.verify_printer_by_filter(name=name, vid=vid, pid=pid)):
                return True
        return False
    except Exception as e:
        print("Error filtering connected_devices: ", e)

async def filter_connected_devices(all_devices: list[PrinterInfoResponse]):
    try:
        filtered_list = []
        connected_devices = await lista_puertos_com_usb_vid_pid()
        print(connected_devices)
        for device in all_devices:
            for connect_device in connected_devices:
                if(device["conn"] != None):
                    if device["conn"]["vid"] == connect_device["vid"] and device["conn"]["pid"] == connect_device["pid"]:
                        port = device["info"].get("pPortName")
                        if port and port.endswith(":"):
                            port = port[:-1]
                            device["info"]["pPortName"] = port
                        filtered_list.append(device)
                elif (round((len(set(connect_device["name"].split()).intersection(set(device["name"].split()))) / max(len(set(connect_device["name"].split())), len(set(device["name"].split())))) * 100, 2) >= 30): #Porcentaje de coincidencias arriba del 30%
                    #agregar los datos de conn:
                    if device["conn"] == None:
                        device["conn"] = {
                            "vid": connect_device["vid"],
                            "pid": connect_device["pid"],
                            "bus": None,
                            "address": None,
                            "manufacturer": None,
                            "product": None,
                            "serial_number": None,
                            "class_dev": None,
                        }
                    filtered_list.append(device)
                else:
                    pass
        
        return filtered_list
                
    except Exception as e:
        print("Error filtering connected_devices: ", e)

async def get_printers_info(printer_names : List[any], level_info):
    try:
        printer_info_list = []
        for printer_name in printer_names:
            h_printer = win32print.OpenPrinter(printer_name)
            try:
                printer_info = win32print.GetPrinter(h_printer, level_info)
                printer_info_clean = clean_dict(printer_info)

                port = printer_info_clean.get("pPortName")
                if port and port.endswith(":"):
                    port = port[:-1]

                conn = match_printer_by_com_port(port)

                printer_info_list.append({
                    "name": printer_name,
                    "info": printer_info_clean,
                    "conn": conn,
                    "status": await check_registered_printer(name=printer_name, conn=conn)
                })
            finally:
                win32print.ClosePrinter(h_printer)
        return await filter_connected_devices(printer_info_list)
    except Exception as e:
        print(f"Error getting printers info: {e}")
        return None
