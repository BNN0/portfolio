Gestion de basculas Rice Lake, generacion de PDF y etiquetas Zebra

# Basculas Endpoints
## Características

- Soporta modelos IQ+355-A2 y 720i2A
- Conexión TCP/IP y Serial
- Base de datos SQLite integrada
- Registro de logs

## Instalación

### Prerrequisitos

- Python 3.12 o superior
- pip (gestor de paquetes de Python)

### 1. Crear entorno virtual (recomendado pero opcional)

```bash
# Crear entorno virtual
python -m venv venv

# Activar entorno virtual
# En Windows:
venv\Scripts\activate

# En Linux/Mac:
source venv/bin/activate
```

### 2. Instalar dependencias

```bash
pip install -r requirements.txt
```

## ⚙️ Configuración

### Configuración de Básculas

La API soporta dos tipos de conexión:

#### Conexión TCP/IP (Recomendada)
```json
{
  "scale_id": "bascula_01",
  "model": "IQ+355-A2",
  "connection_type": "tcp",
  "address": "192.168.1.100",
  "port": 4001
}
```

#### Conexión Serial
```json
{
  "scale_id": "bascula_02", 
  "model": "720i2A",
  "connection_type": "serial",
  "address": "COM1",
  "baudrate": 9600
}
```

> **Nota:** En Linux, usar `/dev/ttyUSB0` en lugar de `COM1`

### Estructura de Archivos

```
fiverrproject1/
├── src
   ├── scalesRL
      ├── Controllers
         └── RiceLakeController.py # Controlador de los endpoinds para basculas
      ├── Database
         └── db_config.py # Configuración de base de datos
      ├── Models
         └── models.py # Modelos/entidades usadas en el proyecto
   ├── main.py              # Ubicación de endpoints
   ├── scale_config.db    # Base de datos (se crea automáticamente)
├── requirements.txt     # Dependencias
└── README.md           # Este archivo
```

##  Inicio Rápido

### 1. Iniciar los endpoints

```bash
python main.py
```

Las APIs estarán disponible en: `http://localhost:8000`

### 2. Documentación Swagger

Visita `http://localhost:8000/docs` para ver la documentación completa con Swagger.

## Uso de los endpoints

### 1. Registrar Báscula

**POST** `/scales/register`

Registra una nueva báscula en el sistema.

**Parámetros de entrada:**
```json
{
  "scale_id": "string",
  "model": "string",
  "connection_type": "string",
  "address": "string",
  "port": integer,
  "baudrate": integer
}
```

**Ejemplo:**
```bash
curl -X POST "http://localhost:8000/scales/register" \
  -H "Content-Type: application/json" \
  -d '{
    "scale_id": "bascula_recepcion",
    "model": "IQ+355-A2",
    "connection_type": "tcp",
    "address": "192.168.1.100",
    "port": 4001
  }'
```

**Respuesta:**
```json
{
  "message": "Báscula bascula_recepcion registrada exitosamente"
}
```

### 2. Obtener Lectura de Peso

**GET** `/scales/{scale_id}/{get_type}`

Obtiene una lectura de peso de la báscula especificada.

**Parámetros:**
- `scale_id`: ID de la báscula registrada
- `get_type`: Tipo de lectura (`weight` para peso bruto, `tare` para peso tara)

**Ejemplo - Peso Bruto:**
```bash
curl "http://localhost:8000/scales/bascula_recepcion/weight"
```

**Ejemplo - Peso Tara:**
```bash
curl "http://localhost:8000/scales/bascula_recepcion/tare"
```

**Respuesta:**
```json
{
  "scale_id": "bascula_recepcion",
  "weight": 125.34,
  "unit": "kg",
  "timestamp": "2025-08-01T10:30:45.123456",
  "status": "stable",
  "reading_type": "weight"
}
```

### 3. Calcular Peso Neto

**POST** `/scales/neto`

Calcula el peso neto basado en los valores de peso bruto y tara proporcionados.

**Parámetros de entrada:**
```json
{
  "peso_bruto": float,
  "peso_tara": float,
  "unit": "string"
}
```

**Ejemplo:**
```bash
curl -X POST "http://localhost:8000/scales/neto" \
  -H "Content-Type: application/json" \
  -d '{
    "peso_bruto": 125.34,
    "peso_tara": 85.20,
    "unit": "kg"
  }'
```

**Respuesta:**
```json
{
  "peso_bruto": 125.34,
  "peso_tara": 85.20,
  "peso_neto": 40.14,
  "unit": "kg",
  "calculation_timestamp": "2025-08-01T10:31:00.123456"
}
```

### 4. Listar Básculas

**GET** `/scales`

Lista todas las básculas registradas en el sistema.

**Ejemplo:**
```bash
curl "http://localhost:8000/scales"
```

**Respuesta:**
```json
[
  {
    "scale_id": "bascula_recepcion",
    "model": "IQ+355-A2",
    "connection_type": "tcp",
    "address": "192.168.1.100",
    "port": 4001,
    "baudrate": null,
    "created_at": "2025-08-01T09:00:00.000000"
  }
]
```

### 5. Eliminar Báscula

**DELETE** `/scales/{scale_id}`

Elimina una báscula del sistema.

**Ejemplo:**
```bash
curl -X DELETE "http://localhost:8000/scales/bascula_recepcion"
```

**Respuesta:**
```json
{
  "message": "Báscula bascula_recepcion eliminada"
}
```


## Ejemplo Completo de Uso

### Módulo de Recepción (Compra de Producto)

```bash
# 1. Registrar báscula
curl -X POST "http://localhost:8000/scales/register" \
  -H "Content-Type: application/json" \
  -d '{
    "scale_id": "bascula_recepcion",
    "model": "IQ+355-A2",
    "connection_type": "tcp",
    "address": "192.168.1.100",
    "port": 4001
  }'

# 2. Camión llega cargado - Obtener peso bruto (camión + mercancía)
curl "http://localhost:8000/scales/bascula_recepcion/weight"
# Respuesta: {"weight": 13000.0, "unit": "kg", ...}

# 3. Camión se descarga y regresa vacío - Obtener peso tara (solo camión)
curl "http://localhost:8000/scales/bascula_recepcion/tare"
# Respuesta: {"weight": 10000.0, "unit": "kg", ...}

# 4. Calcular peso neto (mercancía a pagar)
curl -X POST "http://localhost:8000/scales/neto" \
  -H "Content-Type: application/json" \
  -d '{
    "peso_bruto": 13000.0,
    "peso_tara": 10000.0,
    "unit": "kg"
  }'
# Respuesta: {"peso_neto": 3000.0, ...} → La empresa paga 3 toneladas
```

### Módulo de Salida (Venta de Producto)

```bash
# 1. Registrar báscula (si no existe)
curl -X POST "http://localhost:8000/scales/register" \
  -H "Content-Type: application/json" \
  -d '{
    "scale_id": "bascula_salida",
    "model": "720i2A",
    "connection_type": "tcp",
    "address": "192.168.1.101",
    "port": 4002
  }'

# 2. Camión llega vacío - Obtener peso bruto (solo camión)
curl "http://localhost:8000/scales/bascula_salida/weight"
# Respuesta: {"weight": 10000.0, "unit": "kg", ...}

# 3. Camión se carga y sale - Obtener peso tara (camión + mercancía)
curl "http://localhost:8000/scales/bascula_salida/tare"
# Respuesta: {"weight": 12500.0, "unit": "kg", ...}

# 4. Calcular peso neto (mercancía vendida)
curl -X POST "http://localhost:8000/scales/neto" \
  -H "Content-Type: application/json" \
  -d '{
    "peso_bruto": 10000.0,
    "peso_tara": 12500.0,
    "unit": "kg"
  }'
# Respuesta: {"peso_neto": -2500.0, ...} → Cliente paga 2.5 toneladas (valor absoluto)
```

## Base de Datos

La API utiliza SQLite únicamente para almacenar la configuración de las básculas. **No se almacenan lecturas de peso**

**Tabla `scales`:**
- `scale_id`: Identificador único
- `model`: Modelo de báscula
- `connection_type`: Tipo de conexión
- `address`: Dirección IP o puerto serial
- `port`: Puerto TCP (si aplica)
- `baudrate`: Velocidad serial (si aplica)
- `created_at`: Fecha de registro


## Códigos HTTP

| Código | Significado |
|--------|-------------|
| 200 | Operación exitosa |
| 404 | Báscula no encontrada |
| 400 | Error en los datos enviados |
| 500 | Error interno del servidor |

## Solución de Problemas

### Error: "Báscula no encontrada"
- Verificar que la báscula esté registrada: `curl "http://localhost:8000/scales"`
- Usar el `scale_id` correcto en las peticiones

### Error: "No se pudo conectar a la báscula"
- Verificar que la báscula esté encendida y en la red
- Comprobar la dirección IP y puerto
- Para serial: verificar que el puerto esté disponible

### Error: "Error leyendo el peso"
- Verificar conexión física con la báscula
- Revisar que el modelo sea correcto (IQ+355-A2 o 720i2A)
- Comprobar logs del servidor para más detalles

### La API no responde
- Verificar que esté ejecutándose: `curl "http://localhost:8000/scales"`
- Revisar que el puerto 8000 no esté ocupado
- Consultar logs en la terminal donde se ejecuta `python main.py`

## Logs y Debugging

Los logs de la API se muestran en la terminal. Para más información:

```bash
# Ejecutar con logs detallados
python -c "
import logging
logging.basicConfig(level=logging.DEBUG)
exec(open('main.py').read())
"
```

## Soporte

Para reportar problemas o solicitar funcionalidades:

1. Revisar la documentación en `http://localhost:8000/docs`
2. Verificar los logs de la aplicación
3. Consultar la base de datos SQLite directamente si es necesario

---

**Versión:** 2.1.0  
**Modelos Soportados:** Rice Lake IQ+355-A2, 720i2A  
**Python:** 3.12+  
**Última Actualización:** Agosto 2025


# Impresión de etiquetas - Endpoint

## Características

- Conexión TCP directa a impresoras ZPL (Zebra, etc.)
- Envío de código ZPL
- Soporte para múltiples copias
- Logging completo de operaciones
- Documentación automática con Swagger

## Requisitos

- Python 3.8+
- FastAPI
- Impresora compatible con ZPL (Zebra ZT, GK, etc.)
- Red TCP accesible a la impresora

## Instalación

2. **Crear entorno virtual (recomendado pero opcional)**:
```bash
python -m venv venv
source venv/bin/activate  # En Windows: venv\Scripts\activate
```

3. **Instalar dependencias**:
```bash
pip install -r requirements.txt
```

4. **Ejecutar la API**:
```bash
python main.py
# o
uvicorn main:app --reload --host 0.0.0.0 --port 8001 #Ubicarse dentro de la carpeta printer
```

## 🌐 Endpoint Disponible

### **Base URL**: `http://localhost:8001`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/docs` | Documentación Swagger interactiva |
| POST | `/api/printer/send-zpl` | Envía código ZPL |

## Uso de la API

### 1. Enviar Código ZPL

**Endpoint**: `POST /api/printer/send-zpl`

```json
{
  "printer_config": {
    "ip": "127.0.0.9",
    "port": 9100,
    "timeout": 10
  },
  "producto": "Producto 1",
  "fecha": "08/08/2025",
  "boleta": "3193",
  "cliente": "Nombre cliente",
  "destino": "Nombre destino",
  "placas": "AN311N",
  "vehiculo": "Nombre vehiculo",
  "chofer": "Nombre chofer",
  "copias": 1
}
```

## Configuración de Impresora

### Parámetros de Conexión

- **IP**: Dirección IP de la impresora en la red
- **Puerto**: Generalmente 9100 para impresoras Zebra
- **Timeout**: Tiempo límite de conexión (segundos)

### Puertos Comunes por Marca

| Marca | Puerto Típico | Notas |
|-------|---------------|-------|
| Zebra | 9100 | Puerto estándar TCP |
| Datamax | 9100 | Compatible ZPL |
| TSC | 9100 | Modo ZPL |
| Honeywell | 9100 | Modo ZPL emulado |

## Solución de Problemas

### Errores Comunes

1. **"Conexión rechazada"**
   - Verificar IP y puerto de la impresora
   - Confirmar que la impresora está encendida y conectada
   - Revisar firewall/red

2. **"Timeout de conexión"**
   - Aumentar el valor de timeout
   - Verificar latencia de red
   - Confirmar que la impresora no esté ocupada

3. **"No imprime nada"**
   - Verificar sintaxis del código ZPL
   - Confirmar que hay papel y ribbon (si es térmica transferencia)
   - Revisar configuración de impresora

### Verificación de Impresora

```bash
# Ping a la impresora
ping 192.168.1.100

# Verificar puerto abierto
telnet 192.168.1.100 9100
```

## Consideraciones de Seguridad

- La API no incluye autenticación por defecto
- Restringir acceso por IP si es necesario

## Monitoreo y Logging

La API incluye logging automático de:
- Conexiones exitosas/fallidas
- Bytes enviados
- Errores de red
- Timeouts

Logs disponibles en consola y pueden configurarse para archivos.

---

**Versión:** 1.0.0  
**Python:** 3.8+  
**Última Actualización:** Agosto 2025

# API Generador de Certificados PDF

API para generar certificados de peso y calidad en formato PDF usando FastAPI.

## Endpoint Principal

### POST `/generate-certificate`

Genera un certificado PDF con los datos proporcionados.

**URL**: `http://localhost:8000/generate-certificate`  
**Método**: POST  
**Content-Type**: `application/json`  
**Respuesta**: Archivo PDF

## Estructura del JSON de Petición

```json
{
  "boleta_no": "123459",
  "fecha": "01/01/2024",
  "productor": "Nombre del Productor",
  "producto": "Nombre del Producto",
  "procedencia": "Lugar de Procedencia",
  "vehiculo": "Descripción del Vehículo",
  "placas": "ABC-1234",
  "chofer": "Nombre del Chofer",
  "analisis": [
    {
      "tipo": "HUMEDAD",
      "porcentaje": 10.5,
      "castigo": 12.0
    },
    {
      "tipo": "IMPUREZA",
      "porcentaje": 8.2,
      "castigo": 10.5
    },
    {
      "tipo": "DAÑO",
      "porcentaje": null,
      "castigo": 5.0
    }
  ],
  "pesos_info1": {
    "peso_bruto": 1000.0,
    "peso_tara": 200.0,
    "peso_neto": 800.0,
    "fecha_completa": "01/01/2024 12:00:00"
  },
  "pesos_info2": {
    "deduccion": 50.0,
    "peso_neto_analizado": 750.0
  }
}
```

### Descripción de Campos

#### Campos Principales
- `boleta_no` (string): Número de boleta único
- `fecha` (string): Fecha del certificado
- `productor` (string): Nombre del productor
- `producto` (string): Tipo de producto
- `procedencia` (string): Lugar de origen
- `vehiculo` (string): Descripción del vehículo
- `placas` (string): Número de placas del vehículo
- `chofer` (string): Nombre del chofer

#### Array de Análisis
- `tipo` (string): Tipo de análisis (HUMEDAD, IMPUREZA, DAÑO, QUEBRADO, PESO ESPECIFICO, GRANO VERDE, YODO, CROMATOGRAFIA, MANCHADO, VIEJO, OTROS GRANOS)
- `porcentaje` (float, opcional): Porcentaje encontrado
- `castigo` (float, opcional): Valor del castigo aplicado

#### Información de Pesos 1
- `peso_bruto` (float): Peso bruto en kilogramos
- `peso_tara` (float): Peso de la tara en kilogramos
- `peso_neto` (float): Peso neto en kilogramos
- `fecha_completa` (string): Fecha y hora completa del pesaje

#### Información de Pesos 2
- `deduccion` (float): Deducción aplicada en kilogramos
- `peso_neto_analizado` (float): Peso neto final después del análisis

## Integración en Frontend

### JavaScript Vanilla / Fetch API

```javascript
async function generateCertificate(data) {
    try {
        const response = await fetch('http://localhost:8000/generate-certificate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`Error: ${response.status}`);
        }

        // Obtener el blob del PDF
        const blob = await response.blob();
        
        // Crear URL para descarga
        const url = window.URL.createObjectURL(blob);
        
        // Crear elemento de descarga
        const a = document.createElement('a');
        a.href = url;
        a.download = `BoletaNo${data.boleta_no}.pdf`;
        document.body.appendChild(a);
        a.click();
        
        // Limpiar
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        
    } catch (error) {
        console.error('Error generando certificado:', error);
        alert('Error al generar el certificado');
    }
}

// Ejemplo de uso
const certificateData = {
    boleta_no: "123459",
    fecha: "01/01/2024",
    productor: "Mi Productor",
    // ... resto de los datos
};

generateCertificate(certificateData);
```

### React con Axios

```jsx
import axios from 'axios';

const CertificateGenerator = () => {
    const [loading, setLoading] = useState(false);

    const generateCertificate = async (data) => {
        setLoading(true);
        try {
            const response = await axios.post(
                'http://localhost:8000/generate-certificate',
                data,
                {
                    responseType: 'blob', // Importante para archivos
                    headers: {
                        'Content-Type': 'application/json',
                    }
                }
            );

            // Crear blob y descargar
            const blob = new Blob([response.data], { type: 'application/pdf' });
            const url = window.URL.createObjectURL(blob);
            
            const link = document.createElement('a');
            link.href = url;
            link.download = `BoletaNo${data.boleta_no}.pdf`;
            link.click();
            
            window.URL.revokeObjectURL(url);
            
        } catch (error) {
            console.error('Error:', error);
            alert('Error al generar el certificado');
        } finally {
            setLoading(false);
        }
    };

    return (
        <button 
            onClick={() => generateCertificate(certificateData)}
            disabled={loading}
        >
            {loading ? 'Generando...' : 'Descargar Certificado'}
        </button>
    );
};
```

### Vue.js

```vue
<template>
  <button @click="generateCertificate" :disabled="loading">
    {{ loading ? 'Generando...' : 'Descargar Certificado' }}
  </button>
</template>

<script>
export default {
  data() {
    return {
      loading: false
    }
  },
  methods: {
    async generateCertificate() {
      this.loading = true;
      try {
        const response = await fetch('http://localhost:8000/generate-certificate', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(this.certificateData)
        });

        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
        a.download = `BoletaNo${this.certificateData.boleta_no}.pdf`;
        a.click();
        
        URL.revokeObjectURL(url);
        
      } catch (error) {
        console.error('Error:', error);
      } finally {
        this.loading = false;
      }
    }
  }
}
</script>
```

### jQuery

```javascript
function generateCertificate(data) {
    $.ajax({
        url: 'http://localhost:8000/generate-certificate',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        xhrFields: {
            responseType: 'blob'
        },
        success: function(blob) {
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `BoletaNo${data.boleta_no}.pdf`;
            a.click();
            URL.revokeObjectURL(url);
        },
        error: function(xhr, status, error) {
            console.error('Error:', error);
            alert('Error al generar el certificado');
        }
    });
}
```

## Consideraciones Importantes

### CORS
Si tu frontend está en un dominio diferente, asegúrate de configurar CORS en la API:

```python
from fastapi.middleware.cors import CORSMiddleware

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000"],  # Ajusta según tu frontend
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
```

### Manejo de Errores
La API puede devolver los siguientes códigos de error:
- `400`: Datos de entrada inválidos
- `500`: Error interno del servidor

### Validación de Datos
- Los campos `porcentaje` y `castigo` en el array `analisis` son opcionales
- Los valores numéricos deben ser válidos (no NaN o infinito)
- Las fechas deben estar en formato string

## Ejemplo Completo de Datos

```json
{
  "boleta_no": "123459",
  "fecha": "15/09/2025",
  "productor": "Agricultura San Luis S.A.",
  "producto": "Soja",
  "procedencia": "San Luis Potosí",
  "vehiculo": "Camión Torton",
  "placas": "SLP-1234",
  "chofer": "Juan Pérez García",
  "analisis": [
    {"tipo": "HUMEDAD", "porcentaje": 13.5, "castigo": 2.0},
    {"tipo": "IMPUREZA", "porcentaje": 1.2, "castigo": 0.5},
    {"tipo": "DAÑO", "porcentaje": 2.1, "castigo": 1.0},
    {"tipo": "QUEBRADO", "porcentaje": 5.0, "castigo": 0.0},
    {"tipo": "PESO ESPECIFICO", "porcentaje": 78.5, "castigo": 0.0}
  ],
  "pesos_info1": {
    "peso_bruto": 25000.0,
    "peso_tara": 8500.0,
    "peso_neto": 16500.0,
    "fecha_completa": "15/09/2025 14:30:00"
  },
  "pesos_info2": {
    "deduccion": 125.5,
    "peso_neto_analizado": 16374.5
  }
}
```

La respuesta será un archivo PDF listo para descargar con el nombre `Boleta_NoBoleta-Fecha_Hora.pdf`.
