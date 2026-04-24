from fastapi import FastAPI, HTTPException
from datetime import datetime
import sqlite3
import logging
from Models.models import ScaleConfig, NetWeightRequest
from Controllers.RiceLakeController import RiceLakeScale
from Database.db_config import init_database

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="Rice Lake Scales API", version="1.0.0")

# Configuraciones de básculas
scales_config = {}
active_scales = {}

@app.on_event("startup")
async def startup_event():
    init_database()
    await load_scales_from_db()

#Carga las configuraciones de básculas al inciar servidor
async def load_scales_from_db():
    conn = sqlite3.connect('scale_config.db')
    cursor = conn.cursor()
    cursor.execute('SELECT * FROM scales')
    rows = cursor.fetchall()
    conn.close()
    
    for row in rows:
        config = ScaleConfig(
            scale_id=row[0],
            model=row[1],
            connection_type=row[2],
            address=row[3],
            port=row[4],
            baudrate=row[5]
        )
        scales_config[config.scale_id] = config
        active_scales[config.scale_id] = RiceLakeScale(config)
        logger.info(f"Báscula {config.scale_id} cargada desde base de datos")

# Endpoints

# Registro de nueva bascula en base de datos
@app.post("/scales/register")
async def register_scale(config: ScaleConfig):
    try:
        conn = sqlite3.connect('scale_config.db')
        cursor = conn.cursor()
        cursor.execute('''
            INSERT OR REPLACE INTO scales 
            (scale_id, model, connection_type, address, port, baudrate)
            VALUES (?, ?, ?, ?, ?, ?)
        ''', (config.scale_id, config.model, config.connection_type, 
              config.address, config.port, config.baudrate))
        conn.commit()
        conn.close()
        
        # Crear instancia de báscula
        scale = RiceLakeScale(config)
        scales_config[config.scale_id] = config
        active_scales[config.scale_id] = scale
        
        # Probar conexión
        if await scale.connect():
            scale.disconnect()
            return {"message": f"Báscula {config.scale_id} registrada exitosamente"}
        else:
            raise HTTPException(status_code=400, detail="No se pudo conectar a la báscula")
            
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# Obtener lectura de la báscula
@app.get("/scales/{scale_id}/{get_type}")
async def get_scale_reading(scale_id: str, get_type: str):
    if get_type not in ["weight", "tare"]:
        raise HTTPException(status_code=400, detail="get_type debe ser 'weight' o 'tare'")
    
    if scale_id not in active_scales:
        raise HTTPException(status_code=404, detail="Báscula no encontrada")
    
    scale = active_scales[scale_id]
    reading = await scale.read_weight()
    
    if reading:
        return {
            "scale_id": reading.scale_id,
            "weight": reading.weight,
            "unit": reading.unit,
            "timestamp": reading.timestamp,
            "status": reading.status,
            "reading_type": get_type
        }
    else:
        raise HTTPException(status_code=500, detail="Error leyendo el peso")

# Calculo del peso neto
@app.post("/scales/neto")
async def calculate_net_weight(request: NetWeightRequest):
    try:
        peso_neto = request.peso_bruto - request.peso_tara
        
        return {
            "peso_bruto": request.peso_bruto,
            "peso_tara": request.peso_tara,
            "peso_neto": peso_neto,
            "unit": request.unit,
            "calculation_timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error calculando peso neto: {e}")

# Obtener todas las basculas registradas
@app.get("/scales")
async def list_scales():
    conn = sqlite3.connect('scale_config.db')
    cursor = conn.cursor()
    cursor.execute('SELECT * FROM scales')
    rows = cursor.fetchall()
    conn.close()
    
    scales = []
    for row in rows:
        scales.append({
            "scale_id": row[0],
            "model": row[1],
            "connection_type": row[2],
            "address": row[3],
            "port": row[4],
            "baudrate": row[5],
            "created_at": row[6]
        })
    
    return scales

# Eliminar una bascula registrada
@app.delete("/scales/{scale_id}")
async def remove_scale(scale_id: str):
    if scale_id in active_scales:
        active_scales[scale_id].disconnect()
        del active_scales[scale_id]
        del scales_config[scale_id]
    
    conn = sqlite3.connect('scale_config.db')
    cursor = conn.cursor()
    cursor.execute('DELETE FROM scales WHERE scale_id = ?', (scale_id,))
    conn.commit()
    conn.close()
    
    return {"message": f"Báscula {scale_id} eliminada"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)