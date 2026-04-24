from pydantic import BaseModel
from datetime import datetime
from fastapi import File, Form, UploadFile
from typing import Annotated

class PdfProcessingRequest(BaseModel):
    filename: str
    object_name: str # Name of the object in cloud storage
    bucket_name: str # Bucket name for cloud storage

class PdfProcessingResponse(BaseModel):
    status: str  # "success", "error"
    message: str # Detailed message about the processing result
    processing_info: dict = None # Dictionary with detailed information (size, DPI, blank pages)

class AIPrompt(BaseModel):
    prompt_text: str