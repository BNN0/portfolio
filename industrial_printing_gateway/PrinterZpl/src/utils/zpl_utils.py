import requests
import io
from PIL import Image, ImageTk, ImageDraw, ImageFont
import re
from datetime import datetime

# ==================== PALABRAS RESERVADAS ====================
RESERVED_KEYWORDS = {"date", "time", "timed", "SERIAL"}

def resolve_reserved_keywords(zpl, now=None, part_no=None, master_serial=None):
    if now is None:
        now = datetime.now()
    
    # Formatear valores
    date_val = now.strftime("%d/%m/%Y")
    time_val = now.strftime("%H:%M")
    timed_val = now.strftime("%H:%M:%S")
    
    # Reemplazos básicos
    res = zpl.replace("{date}", date_val)
    res = res.replace("{time}", time_val)
    res = res.replace("{timed}", timed_val)
        
    # Reemplazar {SERIAL}
    if "{SERIAL}" in res:
        if master_serial:
            res = res.replace("{SERIAL}", master_serial)
        elif part_no:
            # Fallback en caso de pruebas previas
            prefix = str(part_no)[:4].upper()
            date_suffix = now.strftime("%d%m%y")
            res = res.replace("{SERIAL}", f"{prefix}{date_suffix}0000")
    
    return res

def render_zpl_offline(zpl, width=4, height=6, dpmm=8):
    # Dimensiones en píxeles
    px_w = int(width * 25.4 * dpmm)
    px_h = int(height * 25.4 * dpmm)
    
    # Crear lienzo blanco
    img = Image.new("RGB", (px_w, px_h), "white")
    draw = ImageDraw.Draw(img)
    
    try:
        font = ImageFont.truetype("arial.ttf", 24)
    except:
        font = ImageFont.load_default()

    elements = re.findall(r"\^FO(\d+),(\d+)\^FD(.*?)\^FS", zpl, re.DOTALL)
    
    for x, y, text in elements:

        pos_x = int(x)
        pos_y = int(y)
        draw.text((pos_x, pos_y), text, fill="black", font=font)
        
    return img

def render_zpl_image(zpl, width=4, height=6, dpmm=8):

    url = f"http://api.labelary.com/v1/printers/{dpmm}dpmm/labels/{width}x{height}/0/"
    try:
        # Intentar con Labelary (Online)
        response = requests.post(url, data=zpl, timeout=2) # Timeout corto para no colgar la UI
        if response.status_code == 200:
            image_data = response.content
            return Image.open(io.BytesIO(image_data))
        else:
            print(f"Labelary devolvió error {response.status_code}, usando fallback offline.")
    except Exception as e:
        print(f"Sin conexión a Labelary ({e}), usando fallback offline.")
    
    # Fallback Offline
    return render_zpl_offline(zpl, width, height, dpmm)
