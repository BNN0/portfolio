from pydantic import BaseModel
from datetime import datetime
from typing import Optional, List

#Modelos de datos
class ScaleReading(BaseModel):
    scale_id: str
    weight: float
    unit: str
    timestamp: datetime
    status: str

class ScaleConfig(BaseModel):
    scale_id: str
    model: str
    connection_type: str  
    address: str 
    port: Optional[int] = None
    baudrate: Optional[int] = 9600

class NetWeightRequest(BaseModel):
    peso_bruto: float
    peso_tara: float
    unit: str = "kg"
