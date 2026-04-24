from __future__ import annotations

import base64
import mimetypes
import os
import shutil
import tempfile
from typing import Any, Dict, List, Tuple

import pandas as pd

from ..config import settings
from ..models import DocumentExtractionResponse, FULL_SYSTEM_PROMPT
from ..state import PROCESSING_RESULTS, PROCESSING_STATUS


def excel_to_text(file_path: str) -> str:
    try:
        xls = pd.ExcelFile(file_path)
        output: List[str] = []
        for sheet_name in xls.sheet_names:
            df = pd.read_excel(xls, sheet_name=sheet_name)
            output.append(f"--- SHEET: {sheet_name} ---")
            if len(df) > 500:
                df = df.head(500)
                output.append("(Showing first 500 rows only)")
            output.append(df.to_string(index=False))
        return "\n\n".join(output)
    except Exception as e:
        return f"[Error extracting Excel text: {str(e)}]"


def _detect_mime(path: str) -> str:
    mime_type, _ = mimetypes.guess_type(path)
    if path.lower().endswith(".xlsx"):
        mime_type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    elif path.lower().endswith(".xls"):
        mime_type = "application/vnd.ms-excel"
    return mime_type or "application/octet-stream"


def save_upload_to_temp(upload_file) -> Tuple[str, str]:
    suffix = os.path.splitext(upload_file.filename)[1]
    with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
        shutil.copyfileobj(upload_file.file, tmp)
        return upload_file.filename, tmp.name


async def process_document_task(filename: str, temp_path: str, s3_client, bucket_name: str):
    try:
        # Lazy imports so the app can start even if LLM deps
        # aren't installed yet in the current environment.
        from langchain_core.messages import HumanMessage, SystemMessage
        from langchain_google_genai import ChatGoogleGenerativeAI

        PROCESSING_STATUS[filename] = 5  # Iniciando...

        PROCESSING_STATUS[filename] = 15  # Subiendo a S3...
        with open(temp_path, "rb") as f:
            s3_client.upload_fileobj(f, bucket_name, filename)

        PROCESSING_STATUS[filename] = 30  # Leyendo archivo local...
        mime_type = _detect_mime(temp_path)

        with open(temp_path, "rb") as f:
            file_bytes = f.read()
            encoded_file = base64.b64encode(file_bytes).decode("utf-8")

        PROCESSING_STATUS[filename] = 50  # Consultando AI Agent...

        model = ChatGoogleGenerativeAI(
            model=settings.gemini_model,
            temperature=0.4,
            max_retries=2,
        )

        is_excel = filename.lower().endswith((".xlsx", ".xls"))
        if is_excel:
            content: List[Dict[str, Any]] = [
                {"type": "text", "text": f"DOCUMENT CONTENT (EXCEL):\n\n{excel_to_text(temp_path)}"}
            ]
        else:
            content = [{"type": "media", "data": encoded_file, "mime_type": mime_type}]

        messages = [
            SystemMessage(content=[{"type": "text", "text": FULL_SYSTEM_PROMPT}]),
            HumanMessage(content=content),
        ]

        model_structured = model.with_structured_output(DocumentExtractionResponse)
        response = model_structured.invoke(messages)
        result_dict = response.model_dump()

        PROCESSING_RESULTS[filename] = result_dict
        PROCESSING_STATUS[filename] = 100  # Finalizado
    except Exception as e:
        PROCESSING_STATUS[filename] = -1
        PROCESSING_RESULTS[filename] = {"error": str(e)}
    finally:
        if os.path.exists(temp_path):
            try:
                os.remove(temp_path)
            except Exception:
                pass

