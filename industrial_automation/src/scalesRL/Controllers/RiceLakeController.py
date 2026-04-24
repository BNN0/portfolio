from Models.models import ScaleConfig, ScaleReading
from typing import Optional
import serial
import socket
import asyncio
import logging
from datetime import datetime
import re
import time

logger = logging.getLogger(__name__)

class RiceLakeScale:    
    def __init__(self, config: ScaleConfig):
        self.config = config
        self.connection = None

    async def connect(self):
        try:
            if self.config.connection_type == "serial":
                self.connection = serial.Serial(
                    port=self.config.address,
                    baudrate=self.config.baudrate,
                    timeout=2,
                    inter_byte_timeout=0.5
                )
                # Limpiar buffer al conectar
                self.connection.reset_input_buffer()
                self.connection.reset_output_buffer()
                
            elif self.config.connection_type == "tcp":
                self.connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                self.connection.settimeout(5)
                self.connection.connect((self.config.address, self.config.port))
            
            logger.info(f"Conectado a báscula {self.config.scale_id}")
            return True
        except Exception as e:
            logger.error(f"Error conectando a {self.config.scale_id}: {e}")
            return False

    async def read_weight_iq355(self) -> Optional[dict]:
        """
        Método específico para IQ+355-A2 que requiere doble petición
        """
        try:
            command = b"P\r\n"
            
            # === PRIMERA PETICIÓN (Wake-up/Inicialización) ===
            print("📡 Enviando primera petición (wake-up) a IQ+355-A2...")
            
            self.connection.reset_input_buffer()
            self.connection.write(command)
            self.connection.flush()
            
            # Leer respuesta de wake-up (puede estar vacía o ser inválida)
            await asyncio.sleep(0.2)
            first_response = self.connection.readline()
            
            if first_response:
                decoded = first_response.decode('ascii', errors='ignore').strip()
                print(f"Primera respuesta: '{decoded}' (longitud: {len(decoded)})")
                
                # Si la primera respuesta ya es válida, usarla
                if self.is_valid_weight_response(decoded):
                    print("✓ Primera respuesta ya es válida")
                    return self.parse_weight_response(first_response)
            else:
                print("Primera respuesta vacía (normal para IQ+355-A2)")
            
            # === SEGUNDA PETICIÓN (Datos reales) ===
            print("📡 Enviando segunda petición (datos reales)...")
            
            # Pausa entre peticiones
            await asyncio.sleep(0.3)
            
            # Limpiar buffer antes de segunda petición
            self.connection.reset_input_buffer()
            
            # Segunda petición
            self.connection.write(command)
            self.connection.flush()
            
            # Leer respuesta real
            await asyncio.sleep(0.2)
            second_response = self.connection.readline()
            
            if second_response:
                decoded = second_response.decode('ascii', errors='ignore').strip()
                print(f"Segunda respuesta: '{decoded}' (longitud: {len(decoded)})")
                
                if decoded:
                    return self.parse_weight_response(second_response)
            
            # === TERCERA PETICIÓN (si la segunda falla) ===
            print("⚠ Segunda respuesta vacía, intentando tercera petición...")
            
            await asyncio.sleep(0.2)
            self.connection.reset_input_buffer()
            self.connection.write(command)
            self.connection.flush()
            
            await asyncio.sleep(0.2)
            third_response = self.connection.readline()
            
            if third_response:
                decoded = third_response.decode('ascii', errors='ignore').strip()
                print(f"Tercera respuesta: '{decoded}' (longitud: {len(decoded)})")
                return self.parse_weight_response(third_response)
                
        except Exception as e:
            logger.error(f"Error en read_weight_iq355: {e}")
            
        return None

    async def read_weight_720i2a(self) -> Optional[dict]:
        """
        Método para 720i2A (comportamiento normal)
        """
        try:
            command = b"W\r\n"
            
            self.connection.reset_input_buffer()
            self.connection.write(command)
            self.connection.flush()
            
            await asyncio.sleep(0.2)
            response = self.connection.readline()
            
            if response:
                decoded = response.decode('ascii', errors='ignore').strip()
                print(f"Respuesta 720i2A: '{decoded}' (longitud: {len(decoded)})")
                return self.parse_weight_response(response)
            
        except Exception as e:
            logger.error(f"Error en read_weight_720i2a: {e}")
            
        return None

    def is_valid_weight_response(self, response: str) -> bool:
        """
        Verifica si una respuesta contiene datos de peso válidos
        """
        if not response or len(response) < 3:
            return False
        
        # Debe tener números y letras
        has_numbers = any(c.isdigit() for c in response)
        has_letters = any(c.isalpha() for c in response)
        
        return has_numbers and has_letters

    async def read_weight(self) -> Optional[ScaleReading]:
        if not self.connection:
            if not await self.connect():
                return None
        
        try:
            # Determinar método según el modelo
            if self.config.model == "IQ+355-A2":
                weight_data = await self.read_weight_iq355()
            elif self.config.model == "720i2A":
                weight_data = await self.read_weight_720i2a()
            else:
                # Método por defecto (usar IQ+355-A2)
                weight_data = await self.read_weight_iq355()
            
            if weight_data:
                return ScaleReading(
                    scale_id=self.config.scale_id,
                    weight=weight_data['weight'],
                    unit=weight_data['unit'],
                    timestamp=datetime.now(),
                    status=weight_data['status']
                )
                
        except Exception as e:
            logger.error(f"Error leyendo peso de {self.config.scale_id}: {e}")
            # En caso de error persistente, reconectar
            self.disconnect()
            
        return None

    def parse_weight_response(self, raw_response) -> Optional[dict]:
        try:
            if isinstance(raw_response, bytes):
                if len(raw_response) == 0:
                    logger.warning("Respuesta vacía recibida")
                    return None
                    
                try:
                    response = raw_response.decode('ascii', errors='ignore').strip()
                    print(f"Parseando respuesta: '{response}'")
                except:
                    return self.parse_binary_response(raw_response)
            else:
                response = raw_response.strip()
            
            if not response:
                return None
            
            # Limpiar respuesta
            clean_response = self.clean_response(response)
            
            # Parseo para formato con espacios (IQ+355-A2 típicamente)
            parts = clean_response.split()
            if len(parts) >= 2:
                try:
                    # Para IQ+355-A2: formato típico "S +00012.34 lb" o "+00012.34 lb"
                    if parts[0].upper() in ['S', 'M', 'T']:  # S=Stable, M=Motion, T=Tare
                        weight_str = parts[1]
                        unit = parts[2] if len(parts) > 2 else parts[-1]
                        status = "stable" if parts[0].upper() == 'S' else "motion"
                    else:
                        weight_str = parts[0]
                        unit = parts[1]
                        status = "stable"
                    
                    # Limpiar peso de signos y espacios
                    weight = float(weight_str.replace('+', '').replace(' ', '').replace('\x00', ''))
                    
                    # Limpiar unidad
                    unit = re.sub(r'[^A-Za-z]', '', unit).upper()
                    if not unit:
                        unit = "LB"  # Default para IQ+355-A2
                    
                    print(f"✓ Parseo con espacios: peso={weight}, unidad={unit}, estado={status}")
                    
                    return {
                        'weight': weight,
                        'unit': unit,
                        'status': status
                    }
                except ValueError as e:
                    print(f"Error en parseo con espacios: {e}")
            
            # Parseo compacto (para 720i2A)
            return self.parse_compact_response(clean_response)
                
        except Exception as e:
            logger.error(f"Error parseando respuesta: {raw_response}, error: {e}")
            
        return None

    def clean_response(self, response: str) -> str:
        """
        Limpia la respuesta de caracteres de control
        """
        # Remover caracteres de control
        control_chars = ['\x02', '\x03', '\r', '\n', '\x00']
        for char in control_chars:
            response = response.replace(char, '')
        
        return response.strip()

    def parse_compact_response(self, response: str) -> Optional[dict]:
        """
        Para formato compacto como "60LG"
        """
        try:
            pattern = r'([+-]?\d*\.?\d+)\s*([A-Za-z]+)'
            match = re.search(pattern, response)
            
            if match:
                weight_str = match.group(1)
                unit = match.group(2)
                weight = float(weight_str)
                
                # Normalizar unidad
                unit = unit.upper()[:2]
                
                return {
                    'weight': weight,
                    'unit': unit,
                    'status': 'stable'
                }
                    
        except Exception as e:
            logger.error(f"Error en parseo compacto: {e}")
            
        return None

    def parse_binary_response(self, raw_response: bytes) -> Optional[dict]:
        try:
            # Filtrar caracteres imprimibles
            filtered_data = []
            for byte_val in raw_response:
                if 32 <= byte_val <= 126:  
                    filtered_data.append(chr(byte_val))
            
            clean_response = ''.join(filtered_data).strip()
            
            if clean_response:
                return self.parse_compact_response(clean_response)
                
        except Exception as e:
            logger.error(f"Error parseando respuesta binaria: {e}")
            
        return None
    
    def disconnect(self):
        if self.connection:
            try:
                if self.config.connection_type == "serial":
                    self.connection.reset_input_buffer()
                    self.connection.reset_output_buffer()
                self.connection.close()
                print("✓ Conexión cerrada")
            except Exception as e:
                logger.warning(f"Error al cerrar conexión: {e}")
            finally:
                self.connection = None