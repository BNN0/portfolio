import os
import io
import re
import traceback
from typing import List
from contextlib import asynccontextmanager
from io import BytesIO
 
os.environ["FLAGS_enable_pir_in_executor"] = "0"
os.environ["FLAGS_use_mkldnn"] = "0"
os.environ["FLAGS_call_stack_level"] = "2"
os.environ["CPU_NUM"] = "1"
os.environ["OMP_NUM_THREADS"] = "1"
os.environ["DISABLE_MODEL_SOURCE_CHECK"] = "1"
os.environ["PADDLE_PDX_DISABLE_MODEL_SOURCE_CHECK"] = "1"
os.environ["HUB_OFFLINE"] = "1"
os.environ["PADDLEHUB_OFFLINE"] = "1"
os.environ["PADDLEX_OFFLINE"] = "1"
os.environ["PADDLEX_MODEL_LOCAL_ONLY"] = "1"
os.environ["PPSTRUCTURE_USE_LOCAL_MODELS"] = "1"
os.environ["PADDLEOCR_MCP_PPOCR_SOURCE"] = "LOCAL"
os.environ["PADDLE_OCR_MODEL_DOWNLOAD"] = "0"
os.environ['FLAGS_enable_pir_api'] = '0'
os.environ['FLAGS_prim_all'] = 'false'

import numpy as np
import cv2
import fitz  # PyMuPDF
from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import PlainTextResponse
from paddleocr import PPStructureV3
import paddle
import pandas as pd

try:
    paddle.set_flags({"FLAGS_use_mkldnn": False})
except Exception:
    pass

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
MODELS_DIR = os.path.join(BASE_DIR, 'src', 'models')

# --- Función de conversión PDF -> imágenes con Fitz a DPI específico ---
def pdf_to_images_fitz(pdf_bytes: bytes, dpi: int) -> List[np.ndarray]:
    # Abrir PDF desde bytes
    pdf_doc = fitz.open(stream=pdf_bytes, filetype="pdf")
    
    # Calcular zoom: DPI / 72 (72 es el DPI estándar de PDF)
    zoom = dpi / 72.0
    mat = fitz.Matrix(zoom, zoom)
    
    images = []
    
    for page_num in range(len(pdf_doc)):
        page = pdf_doc[page_num]

        # Limpiar pagina antes de rasterizar
        page.clean_contents()
        
        pix = page.get_pixmap(matrix=mat, alpha=False, colorspace=fitz.csRGB)

        MAX_WIDTH = 2000

        if pix.width > MAX_WIDTH:
            scale = MAX_WIDTH / pix.width
            mat2 = fitz.Matrix(zoom * scale, zoom * scale)
            pix = page.get_pixmap(matrix=mat2, alpha=False, colorspace=fitz.csRGB)
        
        # Fitz Pixmap -> numpy array uint8
        img_array = np.frombuffer(pix.samples, dtype=np.uint8).reshape(
            pix.height, pix.width, pix.n
        )
        
        # Si tiene canal alfa, removerlo
        if pix.n == 4:  # RGBA
            img_array = img_array[:, :, :3]
        
        h, w = img_array.shape[:2]
        new_h = (h // 32) * 32
        new_w = (w // 32) * 32
        if new_h != h or new_w != w:
            img_array = cv2.resize(img_array, (new_w, new_h))
            
        images.append(img_array)
    
    pdf_doc.close()
    return images

def minify_markdown_table(md: str) -> str:
    lines = md.split("\n")
    optimized = []
    
    for line in lines:
        if "|" in line:
            # Quitar espacios antes y después de barras verticales
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

        # Leer todas las hojas: devuelve un dict {sheet_name: DataFrame}
        sheets = pd.read_excel(excel_file, sheet_name=None, dtype=str)

        for sheet_name, df in sheets.items():
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

@asynccontextmanager
async def lifespan(app: FastAPI):
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

# --- Endpoint principal ---
@app.post("/extract-markdown", response_class=PlainTextResponse)
async def extract_markdown(file: UploadFile = File(...), dpi: int = 150):
    try:
        if not file:
            raise HTTPException(status_code=400, detail="Se requiere un archivo PDF o Excel.")

        content = await file.read()
        if not content:
            raise HTTPException(status_code=400, detail="Archivo vacío.")

        # Determinar tipo de archivo
        filename = file.filename.lower()
        is_excel = filename.endswith(('.xls', '.xlsx'))
        is_pdf = filename.endswith('.pdf')

        if not is_excel and not is_pdf:
            raise HTTPException(status_code=400, detail="Formato no soportado. Use PDF o Excel (.xls, .xlsx).")

        # 1) Convertir archivo a imágenes o HTML
        if is_pdf:
            try:
                images = pdf_to_images_fitz(content, dpi=dpi)
            except Exception as e:
                tb = traceback.format_exc()
                print(f"[ERROR] pdf_to_images_fitz falló:\n{tb}")
                raise HTTPException(
                    status_code=500,
                    detail=f"Error al convertir PDF a imágenes: {e}\n\nTraceback:\n{tb}"
                )
        else:  # Excel
            try:
                html_pages = excel_to_markdown(content)
                final_md = "\n\n---PAGE BREAK---\n\n".join(html_pages)
                return PlainTextResponse(
                    content=final_md or "",
                    media_type="text/markdown"
                )
            except Exception as e:
                tb = traceback.format_exc()
                print(f"[ERROR] excel_to_markdown falló:\n{tb}")
                raise HTTPException(
                    status_code=500,
                    detail=f"Error al procesar Excel: {e}\n\nTraceback:\n{tb}"
                )

        pipeline: PPStructureV3 = getattr(app.state, "pipeline", None)
        if pipeline is None:
            raise HTTPException(status_code=500, detail="Pipeline no inicializada.")

        markdown_pages = []

        for i, img_array in enumerate(images):
            # 2) Preprocesar imagen
            try:
                img_processed = preprocess_image(img_array)
            except Exception as e:
                tb = traceback.format_exc()
                print(f"[ERROR] preprocess_image página {i}:\n{tb}")
                raise HTTPException(
                    status_code=500,
                    detail=f"Error preprocesando página {i}: {e}\n\nTraceback:\n{tb}"
                )

            # 3) Ejecutar pipeline OCR
            try:
                outputs = list(pipeline.predict(img_processed))
            except Exception as e:
                tb = traceback.format_exc()
                print(f"[ERROR] pipeline.predict página {i}:\n{tb}")
                raise HTTPException(
                    status_code=500,
                    detail=f"Error en pipeline página {i}: {e}\n\nTraceback:\n{tb}"
                )

            # 4) Extraer markdown
            page_markdowns = []
            for res in outputs:
                md_info = getattr(res, "markdown", None)
                if md_info is None:
                    continue
                
                # Manejo flexible del formato markdown
                if isinstance(md_info, dict):
                    md_texts = md_info.get("markdown_texts", None)
                    if md_texts:
                        if isinstance(md_texts, (list, tuple)):
                            page_markdowns.append("\n".join(str(t) for t in md_texts))
                        else:
                            page_markdowns.append(str(md_texts))
                elif isinstance(md_info, str):
                    page_markdowns.append(md_info)
                else:
                    page_markdowns.append(str(md_info))

            # Concatenar markdown de la página
            page_content = "\n\n".join(page_markdowns)
            if page_content:
                markdown_pages.append(page_content)

        # 5) Unir todas las páginas
        final_md = "\n\n---PAGE BREAK---\n\n".join(markdown_pages)

        return PlainTextResponse(
            content=final_md or "",
            media_type="text/markdown"
        )

    except HTTPException:
        raise
    except Exception as e:
        tb = traceback.format_exc()
        print(f"[ERROR] Excepción no capturada en extract_markdown:\n{tb}")
        raise HTTPException(
            status_code=500,
            detail=f"Error interno inesperado: {e}\n\nTraceback:\n{tb}"
        )

@app.get("/health")
async def health():
    return {"status": "ok", "pipeline": "ready"}