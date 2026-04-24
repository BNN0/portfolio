import requests
import os
from dotenv import load_dotenv
load_dotenv()

ACCESS_TOKEN = os.getenv("WHATSAPP_ACCESS_TOKEN", "").strip()

def normalize_phone_number(phone_number: str) -> str:
    if phone_number.startswith("521"):
        return "52" + phone_number[3:]
    return phone_number

def send_message_to_user(from_id, message):
    try:
        from_id=normalize_phone_number(from_id)
        
        text_payload = {
            "messaging_product": "whatsapp",
            "to": from_id,
            "type": "text",
            "text": {"body": message}
        }

        headers = {
            "Authorization": f"Bearer {ACCESS_TOKEN}",
            "Content-Type": "application/json"
        }
        
        print(f"Enviando mensaje al usuario {from_id}: {message}")
        print(f"Payload: {text_payload}")
        print(f"Headers: {headers}")
        
        response = requests.post(
            os.getenv("WHATSAPP_API_URL").format(phone_number_id=os.getenv("PHONE_NUMBER_ID")),
            headers=headers,
            json=text_payload
        )
        response.raise_for_status()

        print(f"Mensaje enviado exitosamente al usuario {from_id}: {message}")
    except requests.exceptions.RequestException as e:
        print(f"Error al enviar mensaje al usuario {from_id}: {e}")

