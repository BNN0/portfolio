import os
import io
from typing import List
from contextlib import asynccontextmanager

import numpy as np
import cv2
import fitz 
from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import PlainTextResponse, Response
import requests
import base64
from openpyxl import Workbook
from paddleocr import PPStructureV3
import pandas as pd
from typing import List
from io import BytesIO
import re

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
MODELS_DIR = os.path.join(BASE_DIR, 'src', 'models')

# --- Función de conversión PDF -> imágenes con Fitz a DPI específico ---
def pdf_to_images_fitz(pdf_bytes: bytes, dpi: int) -> List[np.ndarray]:
    # Abrir PDF desde bytes
    pdf_doc = fitz.open(stream=pdf_bytes, filetype="pdf")
    
    # Calcular zoom: DPI / 72
    zoom = dpi / 72.0
    mat = fitz.Matrix(zoom, zoom)
    
    images = []
    
    for page_num in range(len(pdf_doc)):
        page = pdf_doc[page_num]

        # Limpiar pagina antes de rasterizar
        page.clean_contents()
        
        pix = page.get_pixmap(matrix=mat, alpha=False)

        MAX_WIDTH = 2000

        if pix.width > MAX_WIDTH:
            scale = MAX_WIDTH / pix.width
            mat2 = fitz.Matrix(scale, scale)
            pix = page.get_pixmap(matrix=mat2,alpha=False)
        
        img_array = np.frombuffer(pix.samples, dtype=np.uint8).reshape(
            pix.height, pix.width, pix.n
        )
        
        if pix.n == 4:  # RGBA
            img_array = img_array[:, :, :3]  
        
        images.append(img_array)
    
    pdf_doc.close()
    return images

def minify_markdown_table(md: str) -> str:
    lines = md.split("\n")
    optimized = []
    
    for line in lines:
        if "|" in line:
            line = re.sub(r'\s*\|\s*', '|', line)
            line = line.strip('|')
            line = f"|{line}|"
        optimized.append(line)
    
    return "\n".join(optimized)

def excel_to_markdown(excel_bytes: bytes) -> List[str]:
    markdown_pages = []

    try:
        # Cargar el archivo en memoria
        excel_file = BytesIO(excel_bytes)

        sheets = pd.read_excel(excel_file, sheet_name=None, dtype=str)

        for sheet_name, df in sheets.items():
            # Normalizar NaN > cadena vacía 
            df = df.fillna("")

            # Convertir a markdown
            markdown = df.to_markdown(index=False)
            markdown = minify_markdown_table(markdown)

            full_markdown = f"### {sheet_name}\n\n{markdown}"

            markdown_pages.append(full_markdown)

    except Exception as e:
        raise ValueError(f"Error al procesar Excel a Markdown: {e}")

    return markdown_pages


# --- Preprocesamiento optimizado ---
def gray_scale(img: np.ndarray) -> np.ndarray:
    return cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

def contrast_bright(img: np.ndarray, contrast: float, bright: int) -> np.ndarray:
    return cv2.addWeighted(img, contrast, np.zeros(img.shape, img.dtype), 0, bright)

def contrast_clahe(img: np.ndarray) -> np.ndarray:
    if len(img.shape) != 2:
        img = gray_scale(img)
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    return clahe.apply(img)

def adaptative_binary(img: np.ndarray) -> np.ndarray:
    binary_gauss = cv2.adaptiveThreshold(
        img, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
        cv2.THRESH_BINARY, 11, 2
    )
    return binary_gauss

def preprocess_image(img: np.ndarray) -> np.ndarray:
    # Convertir a grayscale
    img_gray = gray_scale(img)
    
    # Ajuste de contraste y brillo
    img_contrast = contrast_bright(img_gray, 1.1, 0)
    
    # Mejora de contraste local (CLAHE) - RÁPIDO y efectivo
    img_clahe = contrast_clahe(img_contrast)
    
    # Threshold adaptativo para mejorar OCR
    img_binary = adaptative_binary(img_clahe)
    
    # Convertir de vuelta a RGB para la pipeline
    img_rgb = cv2.cvtColor(img_binary, cv2.COLOR_GRAY2RGB)
    
    return img_rgb

# --- FastAPI con lifespan ---
@asynccontextmanager
async def lifespan(app: FastAPI):
    os.environ["HUB_OFFLINE"] = '1'
    os.environ["PADDLEHUB_OFFLINE"] = '1'
    os.environ["PADDLEX_OFFLINE"] = '1'
    os.environ["PADDLEX_MODEL_LOCAL_ONLY"] = '1'
    os.environ["PPSTRUCTURE_USE_LOCAL_MODELS"] = '1'
    os.environ["PADDLEOCR_MCP_PPOCR_SOURCE"] = "LOCAL"
    os.environ["PADDLE_OCR_MODEL_DOWNLOAD"] = '0'
    os.environ["DISABLE_MODEL_SOURCE_CHECK"] = '1'

    os.environ["PADDLEX_HOME"] = os.path.abspath('src/models/.paddlex')
    os.environ["PADDLEX_CACHE_DIR"] = os.path.abspath('src/models/.paddlex/temp')
    os.environ["PADDLE_HUB_HOME"] = os.path.abspath('src/models/.paddlex/official_models')
    os.environ["HUB_HOME"] = os.path.abspath('src/models/.paddlex/official_models')

    try:
        app.state.pipeline = PPStructureV3(paddlex_config=os.path.join(BASE_DIR, "src/ocr_config.yaml"))
    except Exception as e:
        raise RuntimeError(f"Falló inicializar PPStructureV3: {e}")

    try:
        doc = fitz.open()
        page = doc.new_page(width=595, height=842)  # A4
        page.insert_text((10, 10), "warmup", fontsize=12)
        pdf_bytes = doc.write()
        doc.close()
        
        test_images = pdf_to_images_fitz(pdf_bytes, dpi=200)
        _ = test_images[0]
    except Exception as e:
        import traceback
        print("Warm-up warning:", e)
        traceback.print_exc()

    try:
        yield
    finally:
        try:
            del app.state.pipeline
        except Exception:
            pass

app = FastAPI(lifespan=lifespan)

def json_to_excel(json_data) -> bytes:
    wb = Workbook()
    default_sheet = wb.active
    wb.remove(default_sheet)

    # Convertir a lista si vino un solo dict
    blocks = json_data if isinstance(json_data, list) else [json_data]

    for block in blocks:
        sheets = block.get("excel_sheets", [])

        for sheet in sheets:
            name = sheet.get("sheet_name", "Sheet")[:31]
            headers = sheet.get("headers", [])
            rows = sheet.get("rows", [])

            ws = wb.create_sheet(title=name)
            ws.append(headers)

            for row in rows:
                ws.append(row)

    buffer = BytesIO()
    wb.save(buffer)
    buffer.seek(0)
    return buffer.read()

@app.post("/extract-markdown")
async def extract_markdown(file: UploadFile = File(...), dpi: int = 150):
    if not file:
        raise HTTPException(status_code=400, detail="Se requiere un archivo PDF o Excel.")

    content = await file.read()
    filename = file.filename.lower()
    is_excel = filename.endswith(('.xls', '.xlsx'))
    is_pdf = filename.endswith('.pdf')

    if not is_excel and not is_pdf:
        raise HTTPException(status_code=400, detail="Formato no soportado.")

    if is_excel:
        html_pages = excel_to_markdown(content)
        md_text = "\n\n---PAGE BREAK---\n\n".join(html_pages)
    else:
    
        try:
            images = pdf_to_images_fitz(content, dpi=dpi)
        except Exception as e:
            raise HTTPException(500, f"Error convirtiendo PDF: {e}")

        pipeline: PPStructureV3 = getattr(app.state, "pipeline", None)
        if pipeline is None:
            raise HTTPException(500, "Pipeline no inicializada.")

        markdown_pages = []

        for img_idx, img in enumerate(images):
            img_processed = preprocess_image(img)
            outputs = pipeline.predict(img_processed)

            print(outputs)

            page_md = []
            for res in outputs:
                md_info = getattr(res, "markdown", None)
                if isinstance(md_info, dict):
                    md_texts = md_info.get("markdown_texts", [])
                    if isinstance(md_texts, list):
                        page_md.append("\n".join(md_texts))
                    else:
                        page_md.append(str(md_texts))
                elif isinstance(md_info, str):
                    page_md.append(md_info)

            if page_md:
                markdown_pages.append("\n\n".join(page_md))

        md_text = "\n\n---PAGE BREAK---\n\n".join(markdown_pages)

    try:
        response = requests.post(
            "http://192.168.10.72:5678/webhook/pdf-to-excel",
            json={"text": md_text},
            timeout=900
        )
    except Exception as e:
        raise HTTPException(500, f"Error llamando a N8N: {e}")

    if response.status_code != 200:
        raise HTTPException(
            500,
            f"N8N devolvió error {response.status_code}: {response.text}"
        )

    try:
        excel_json = response.json()
        print(excel_json)
    except Exception:
        raise HTTPException(500, f"Respuesta inválida desde N8N: {response.text}")

    try:
        excel_bytes = json_to_excel(excel_json)
    except Exception as e:
        raise HTTPException(500, f"Error generando Excel: {e}")

    return Response(
        content=excel_bytes,
        media_type="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        headers={
            "Content-Disposition": 'attachment; filename="ocr_output.xlsx"'
        }
    )

@app.get("/health")
async def health():
    return {"status": "ok", "pipeline": "ready"}