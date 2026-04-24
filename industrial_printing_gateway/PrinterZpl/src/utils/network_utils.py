import requests
import json

def send_print_request(printer_name, zpl_codes, api_url="http://localhost:5000/print"):
    payload = {
        "printer_name": printer_name,
        "zpl_code": zpl_codes
    }
    try:
        response = requests.post(api_url, json=payload, timeout=5)
        return response.status_code == 200
    except Exception as e:
        print(f"Error enviando impresión a API: {e}")
        return False
