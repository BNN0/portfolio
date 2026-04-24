from pydantic import BaseModel
from typing import Optional
from fastapi.responses import JSONResponse
from fastapi import status
class requestModel(BaseModel):
    language: str
    request: str
    token:  Optional[str] = ""
    # session: Optional[str] = ""

class botModel(BaseModel):
    thread_id: str
    request: str

class userModel(BaseModel):
    email:str

class sessionModel(BaseModel):
    session:str

class requestErrorModel(BaseModel):
    error: str

# Funcion para lanzar la excepcion HTTP
def handle_error(status_code, message):
    content = {"error": message}
    return JSONResponse(status_code=status_code, content=content)