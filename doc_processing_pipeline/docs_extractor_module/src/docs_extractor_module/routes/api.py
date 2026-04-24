from __future__ import annotations

import io
from typing import Dict, List

import pandas as pd
from fastapi import APIRouter, BackgroundTasks, File, HTTPException, UploadFile
from fastapi.responses import JSONResponse, StreamingResponse

from ..config import settings
from ..services.extraction import process_document_task, save_upload_to_temp
from ..services.storage import create_s3_client, ensure_bucket_exists
from ..state import PROCESSING_RESULTS, PROCESSING_STATUS


def create_api_router() -> APIRouter:
    router = APIRouter()
    _s3_client = None

    def _get_s3_client():
        nonlocal _s3_client
        if _s3_client is None:
            _s3_client = create_s3_client()
        return _s3_client

    @router.post("/api/upload")
    async def upload_files(background_tasks: BackgroundTasks, files: List[UploadFile] = File(...)):
        uploaded_files: List[str] = []

        s3_client = _get_s3_client()
        ensure_bucket_exists(s3_client, settings.minio_bucket_name)

        for file in files:
            temp_path = None
            try:
                filename, temp_path = save_upload_to_temp(file)
                uploaded_files.append(filename)
                PROCESSING_STATUS[filename] = 0  # Recibido
                background_tasks.add_task(
                    process_document_task,
                    filename,
                    temp_path,
                    s3_client,
                    settings.minio_bucket_name,
                )
            except Exception as e:
                return JSONResponse(
                    status_code=500,
                    content={"message": f"Error initiating upload for {file.filename}: {str(e)}"},
                )

        return {"message": "Files received, upload and processing started in background", "files": uploaded_files}

    @router.get("/api/status")
    async def get_status():
        return PROCESSING_STATUS

    @router.get("/api/results")
    async def get_results():
        return PROCESSING_RESULTS

    @router.get("/api/results/{filename}")
    async def get_file_results(filename: str):
        if filename not in PROCESSING_RESULTS:
            if filename in PROCESSING_STATUS:
                status = PROCESSING_STATUS[filename]
                if status == -1:
                    return JSONResponse(
                        status_code=500,
                        content={
                            "message": "Processing failed",
                            "error": PROCESSING_RESULTS.get(filename, {}).get("error"),
                        },
                    )
                return JSONResponse(status_code=202, content={"message": "Processing in progress", "status": status})
            raise HTTPException(status_code=404, detail="Results not found for this file")
        return PROCESSING_RESULTS[filename]

    @router.post("/api/export")
    async def export_excel(data: Dict):
        output = io.BytesIO()

        with pd.ExcelWriter(output, engine="openpyxl") as writer:
            summary_rows = []
            for filename, results in data.items():
                if isinstance(results, dict) and "individual_data" in results:
                    row = {"Archivo": filename}
                    for item in results.get("individual_data", []):
                        label = item.get("field_label") or item.get("field_name")
                        row[str(label)] = item.get("value")
                    summary_rows.append(row)

            if summary_rows:
                df_summary = pd.DataFrame(summary_rows)
                df_summary.to_excel(writer, sheet_name="Resumen_General", index=False)

            for filename, results in data.items():
                if not isinstance(results, dict):
                    continue
                tables = results.get("tables") or []
                for i, table in enumerate(tables):
                    sheet_name = f"{str(filename)[:20]}_T{i + 1}"
                    headers = table.get("headers") or []
                    rows = table.get("rows") or []
                    if headers and rows:
                        df_table = pd.DataFrame(rows, columns=headers)
                        df_table.to_excel(writer, sheet_name=sheet_name[:31], index=False)

        output.seek(0)
        headers = {"Content-Disposition": 'attachment; filename="Extraccion_Documentos.xlsx"'}
        return StreamingResponse(
            output,
            headers=headers,
            media_type="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        )

    return router

