from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from typing import Optional
import socket
import asyncio
from datetime import datetime
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="ZPL Printer API",
    description="API para envío de comandos ZPL a impresoras via TCP",
    version="1.0.0"
)

class PrinterConfig(BaseModel):
    ip: str = Field(..., description="Dirección IP de la impresora")
    port: int = Field(default=9100, description="Puerto TCP de la impresora (default: 9100)")
    timeout: int = Field(default=10, description="Timeout de conexión en segundos")

class ZPLPrintRequest(BaseModel):
    printer_config: PrinterConfig
    producto: str
    fecha: str
    boleta: str
    cliente: str
    destino: str
    placas: str
    vehiculo: str
    chofer: str
    copias: int = Field(default=1, ge=1, le=100, description="Número de copias (1-100)")

class PrintResponse(BaseModel):
    success: bool
    message: str
    printer_ip: str
    timestamp: str

class ZPLPrinterService:

    @staticmethod
    async def send_zpl_command(ip: str, port: int, zpl_code: str, timeout: int = 10) -> dict:
        """
        Envía comando ZPL a la impresora via TCP
        """
        try:
            # Crear socket TCP
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(timeout)
            
            # Conectar a la impresora
            logger.info(f"Conectando a impresora {ip}:{port}")
            sock.connect((ip, port))
            
            # Enviar código ZPL
            zpl_bytes = zpl_code.encode('utf-8')
            sock.send(zpl_bytes)
            logger.info(f"Enviado {len(zpl_bytes)} bytes de código ZPL")
            
            # Cerrar conexión
            sock.close()
            
            return {
                "success": True,
                "message": f"Comando ZPL enviado exitosamente a {ip}:{port}",
                "bytes_sent": len(zpl_bytes)
            }
            
        except socket.timeout:
            error_msg = f"Timeout al conectar con impresora {ip}:{port}"
            logger.error(error_msg)
            return {"success": False, "message": error_msg}
            
        except socket.gaierror as e:
            error_msg = f"Error de resolución DNS para {ip}: {str(e)}"
            logger.error(error_msg)
            return {"success": False, "message": error_msg}
            
        except ConnectionRefusedError:
            error_msg = f"Conexión rechazada por impresora {ip}:{port}"
            logger.error(error_msg)
            return {"success": False, "message": error_msg}
            
        except Exception as e:
            error_msg = f"Error inesperado: {str(e)}"
            logger.error(error_msg)
            return {"success": False, "message": error_msg}
        
        finally:
            try:
                sock.close()
            except:
                pass
    
    @staticmethod
    def generate_label_zpl(producto: str, fecha: str, boleta: str, cliente: str, destino: str, placas: str, vehiculo: str, chofer: str) -> str:
        # Genera un código ZPL para etiqueta
        zpl = '^XA^FX Top section with logo, name and address.' \
        '^CF0,50^FO80,90^FDAceites y Proteinas, S.A. de C.V.' \
        f'^FS^FO50,160^GB700,3,3^FS^CFF,30^FO70,200^FDPRODUCTO : {producto}' \
        f'^FS^FO70,250^FDFECHA    : {fecha}^FS^FO70,300^FDBOLETA   : {boleta}' \
        f'^FS^FO50,350^GB700,3,3^FS^FO70,400^FDCLIENTE  : {cliente}' \
        f'^FS^FO70,450^FDDESTINO  : {destino}^FS^FO70,500^FDPLACAS   : {placas}' \
        f'^FS^FO70,550^FDVEHICULO : {vehiculo}^FS^FO70,600^FDCHOFER   : {chofer}^FS^XZ'
        return zpl

printer_service = ZPLPrinterService()

@app.post("/api/printer/send-zpl", response_model=PrintResponse)
async def send_zpl_command(request: ZPLPrintRequest):
    # Envía código ZPL directamente a la impresora
    try:
        final_zpl = printer_service.generate_label_zpl(
            producto = request.producto,
            fecha = request.fecha,
            boleta = request.boleta,
            cliente = request.cliente,
            destino = request.destino,
            placas =  request.placas,
            vehiculo = request.vehiculo,
            chofer = request.chofer
        )
        if request.copias > 1:
            # Agregar comando de múltiples copias
            if "^PQ" not in final_zpl:
                final_zpl = final_zpl.replace("^XA", f"^XA\n^PQ{request.copias}")
        
        result = await printer_service.send_zpl_command(
            ip=request.printer_config.ip,
            port=request.printer_config.port,
            zpl_code=final_zpl,
            timeout=request.printer_config.timeout
        )
        
        if not result["success"]:
            raise HTTPException(status_code=500, detail=result["message"])
        
        return PrintResponse(
            success=True,
            message=result["message"],
            printer_ip=request.printer_config.ip,
            timestamp=datetime.now().isoformat()
        )
        
    except Exception as e:
        logger.error(f"Error en send_zpl_command: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error al enviar comando ZPL: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001)