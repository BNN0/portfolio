# Invoice Validator API - FastAPI

API REST para validación de facturas logísticas vs documentos SAT, construida con FastAPI.

## 🚀 Características

- ✅ Validación de múltiples archivos PDF/XML
- ✅ Comparación automática de campos
- ✅ Envío de observaciones por correo electrónico
- ✅ Datos de prueba para desarrollo de frontend
- ✅ Documentación interactiva automática (Swagger/OpenAPI)
- ✅ CORS configurado para desarrollo

## 📋 Requisitos

- Python 3.8+
- pip

## 🔧 Instalación

### 1. Clonar o crear el proyecto

```bash
mkdir invoice-validator-api
cd invoice-validator-api
```

### 2. Crear entorno virtual (recomendado)

```bash
# Windows
python -m venv venv
venv\Scripts\activate

# Linux/Mac
python3 -m venv venv
source venv/bin/activate
```

### 3. Instalar dependencias

```bash
pip install -r requirements.txt
```

## 🎯 Ejecución

### Modo desarrollo (con auto-reload)

```bash
python main.py
```

O usando uvicorn directamente:

```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

### Modo producción

```bash
uvicorn main:app --host 0.0.0.0 --port 8000 --workers 4
```

La API estará disponible en:
- **API**: http://localhost:8000
- **Documentación Swagger**: http://localhost:8000/docs
- **Documentación ReDoc**: http://localhost:8000/redoc

## 📡 Endpoints

### 1. Validar Facturas

**POST** `/validate-invoices`

Valida facturas comparando archivos logísticos con documentos SAT.

**Parámetros:**
- `logisticFiles`: Lista de archivos PDF (multipart/form-data)
- `satFiles`: Lista de archivos PDF/XML (multipart/form-data)

**Respuesta:**
```json
{
  "validationId": "uuid",
  "invoiceNumber": "F-12345",
  "files": {
    "logistic": ["Factura_Transporte_AX-587.pdf"],
    "sat": ["CFDI-TVE123456XYZ.xml"]
  },
  "fields": [
    {
      "campo": "Número de Factura",
      "logistica": "F-12345",
      "sat": "F-12345",
      "estado": "success"
    }
  ],
  "details": [...],
  "timestamp": "2024-01-15T10:30:00"
}
```

### 2. Enviar Observaciones

**POST** `/send-observations`

Envía observaciones de validación por correo electrónico.

**Body:**
```json
{
  "email": "destinatario@example.com",
  "validationId": "uuid",
  "invoiceNumber": "F-12345",
  "fields": [...]
}
```

**Respuesta:**
```json
{
  "message": "Observaciones enviadas exitosamente",
  "emailSent": true,
  "timestamp": "2024-01-15T10:35:00"
}
```

### 3. Health Check

**GET** `/health`

Verifica el estado de la API.

## 🧪 Pruebas con cURL

### Validar facturas

```bash
curl -X POST "http://localhost:8000/validate-invoices" \
  -H "accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -F "logisticFiles=@factura1.pdf" \
  -F "logisticFiles=@factura2.pdf" \
  -F "satFiles=@cfdi1.xml"
```

### Enviar observaciones

```bash
curl -X POST "http://localhost:8000/send-observations" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "validationId": "123e4567-e89b-12d3-a456-426614174000",
    "invoiceNumber": "F-12345",
    "fields": [
      {
        "campo": "Total",
        "logistica": "$ 15,950.00 MXN",
        "sat": "$ 15,950.50 MXN",
        "estado": "danger"
      }
    ]
  }'
```

## 📧 Configuración de Email (Opcional)

Para habilitar el envío real de correos, elige uno de estos métodos:

### Opción 1: SendGrid

```bash
pip install sendgrid
export SENDGRID_API_KEY="tu-api-key"
export FROM_EMAIL="noreply@tuempresa.com"
```

### Opción 2: SMTP (Gmail, Outlook, etc.)

```bash
pip install aiosmtplib
export SMTP_HOST="smtp.gmail.com"
export SMTP_PORT="587"
export SMTP_USER="tu-email@gmail.com"
export SMTP_PASSWORD="tu-contraseña-o-app-password"
export FROM_EMAIL="tu-email@gmail.com"
```

### Opción 3: AWS SES

```bash
pip install boto3
export AWS_ACCESS_KEY_ID="tu-access-key"
export AWS_SECRET_ACCESS_KEY="tu-secret-key"
export AWS_REGION="us-east-1"
export FROM_EMAIL="noreply@tuempresa.com"
```

Luego, en `main.py`, importa y usa el servicio de email:

```python
from email_service import email_service

@app.post("/send-observations")
async def send_observations(request: SendObservationsRequest):
    # ... código existente ...
    
    # Enviar email real
    email_sent = await email_service.send_email_sendgrid(
        to_email=request.email,
        invoice_number=request.invoiceNumber,
        fields=request.fields,
        validation_id=request.validationId
    )
    
    # ... resto del código ...
```

## 🔒 Configuración de CORS

Para desarrollo, CORS está configurado para aceptar todas las solicitudes (`allow_origins=["*"]`).

Para producción, modifica en `main.py`:

```python
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "https://tu-dominio.com",
        "https://app.tu-dominio.com"
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
```

## 🔐 Variables de Entorno

Crea un archivo `.env` en la raíz del proyecto:

```env
# Email Configuration
FROM_EMAIL=noreply@tuempresa.com

# SendGrid
SENDGRID_API_KEY=your-sendgrid-api-key

# SMTP
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASSWORD=your-password

# AWS SES
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_REGION=us-east-1

# API Configuration
API_HOST=0.0.0.0
API_PORT=8000
```

## 📁 Estructura del Proyecto

```
invoice-validator-api/
├── main.py                 # API principal con endpoints
├── email_service.py        # Servicio de envío de emails
├── requirements.txt        # Dependencias
├── .env                    # Variables de entorno (no subir a git)
├── .gitignore             # Archivos a ignorar
└── README.md              # Esta documentación
```

## 🐳 Docker (Opcional)

### Dockerfile

```dockerfile
FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 8000

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
```

### Docker Compose

```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "8000:8000"
    environment:
      - FROM_EMAIL=noreply@tuempresa.com
      - SENDGRID_API_KEY=${SENDGRID_API_KEY}
    volumes:
      - ./:/app
    command: uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

Ejecutar con Docker:

```bash
docker-compose up
```

## 🧪 Testing con Postman

Importa la colección de Postman disponible en `/docs` de la API o usa la documentación interactiva de Swagger.

## 🔧 Actualizar el Frontend

En tu archivo TypeScript del frontend, actualiza la URL base:

```typescript
const invoiceValidatorService = new InvoiceValidatorService('http://localhost:8000');
```

Para producción:

```typescript
const invoiceValidatorService = new InvoiceValidatorService('https://api.tudominio.com');
```

## 📊 Datos de Prueba

La API devuelve 3 escenarios diferentes de validación con datos realistas:

1. **Factura F-12345**: Contiene discrepancias en proveedor y total
2. **Factura E-9876**: Contiene discrepancia en fecha
3. **Factura INV-2024-001**: Múltiples discrepancias en datos de proveedor

Los datos se seleccionan aleatoriamente en cada validación.

## 🐛 Troubleshooting

### Error: "Address already in use"

```bash
# Windows
netstat -ano | findstr :8000
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:8000 | xargs kill -9
```

### Error: "ModuleNotFoundError"

```bash
pip install -r requirements.txt --force-reinstall
```

### Error CORS en el frontend

Verifica que el middleware CORS esté configurado correctamente en `main.py`.

## 📝 Logs

Los logs se muestran en la consola con emojis para fácil identificación:
- 📁 Archivos recibidos
- ✅ Validación exitosa
- 📧 Email enviado
- ❌ Errores

## 🚀 Deploy a Producción

### Opción 1: Render.com

1. Conecta tu repositorio
2. Selecciona Python
3. Build Command: `pip install -r requirements.txt`
4. Start Command: `uvicorn main:app --host 0.0.0.0 --port $PORT`

### Opción 2: Railway.app

1. Conecta tu repositorio
2. Railway detectará automáticamente FastAPI
3. Configura las variables de entorno

### Opción 3: AWS EC2

```bash
# En el servidor
sudo apt update
sudo apt install python3-pip python3-venv nginx
git clone tu-repositorio
cd invoice-validator-api
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
pip install gunicorn
gunicorn main:app -w 4 -k uvicorn.workers.UvicornWorker --bind 0.0.0.0:8000
```

## 📄 Licencia

Este proyecto es de código abierto.

## 👥 Soporte

Para reportar problemas o solicitar características, crea un issue en el repositorio.

---

**¡Listo para usar! 🎉**