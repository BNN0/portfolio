# PDF Analyzer API

API de FastAPI para análisis y procesamiento de archivos PDF con almacenamiento en MinIO.

## 🚀 Inicio Rápido con Docker

### Prerrequisitos
- Docker
- Docker Compose

### Ejecutar con Docker Compose

1. **Clonar el repositorio y navegar al directorio:**
   ```bash
   cd compress_clean
   ```

2. **Iniciar los servicios:**
   ```bash
   docker-compose up -d
   ```

   Esto iniciará:
   - **API FastAPI** en `http://localhost:8011`
   - **MinIO** en `http://localhost:9000` (API) y `http://localhost:9001` (Consola)

3. **Verificar que los servicios estén corriendo:**
   ```bash
   docker-compose ps
   ```

4. **Ver logs:**
   ```bash
   # Logs de todos los servicios
   docker-compose logs -f

   # Logs solo de la API
   docker-compose logs -f app

   # Logs solo de MinIO
   docker-compose logs -f minio
   ```

### Acceder a los Servicios

- **API Documentation (Swagger):** http://localhost:8011/docs
- **MinIO Console:** http://localhost:9001
  - Usuario: `minioadmin`
  - Contraseña: `minioadmin`

### Detener los Servicios

```bash
# Detener sin eliminar volúmenes
docker-compose down

# Detener y eliminar volúmenes (⚠️ esto borrará la base de datos)
docker-compose down -v
```

## 📦 Volúmenes Docker

El proyecto utiliza volúmenes Docker para persistir datos:

### `db-data`
- **Propósito:** Almacena la base de datos SQLite (`pdf_analyzer.db`)
- **Ubicación en contenedor:** `/app/src/db`
- **Persistencia:** Los datos se mantienen entre reinicios del contenedor

### `minio-data`
- **Propósito:** Almacena los archivos PDF en MinIO
- **Ubicación en contenedor:** `/data`
- **Persistencia:** Los archivos se mantienen entre reinicios del contenedor

### Gestión de Volúmenes

```bash
# Listar volúmenes
docker volume ls

# Inspeccionar un volumen
docker volume inspect compress_clean_db-data

# Hacer backup de la base de datos
docker cp pdf-analyzer-app:/app/src/db/pdf_analyzer.db ./backup.db

# Restaurar backup
docker cp ./backup.db pdf-analyzer-app:/app/src/db/pdf_analyzer.db
```

## ⚙️ Configuración

### Variables de Entorno

Las variables de entorno se configuran en `docker-compose.yml`. Para desarrollo local sin Docker, puedes crear un archivo `.env`:

```bash
cp .env.example .env
```

Variables disponibles:

| Variable | Descripción | Valor por defecto |
|----------|-------------|-------------------|
| `MINIO_ENDPOINT` | Endpoint del servidor MinIO | `minio:9000` |
| `MINIO_ACCESS_KEY` | Access key de MinIO | `minioadmin` |
| `MINIO_SECRET_KEY` | Secret key de MinIO | `minioadmin` |
| `MINIO_SECURE` | Usar HTTPS para MinIO | `false` |

## 🔧 Desarrollo

### Ejecutar sin Docker (desarrollo local)

1. **Crear entorno virtual:**
   ```bash
   python -m venv .venv
   .venv\Scripts\activate  # Windows
   source .venv/bin/activate  # Linux/Mac
   ```

2. **Instalar dependencias:**
   ```bash
   pip install -r requirements.txt
   ```

3. **Configurar variables de entorno:**
   ```bash
   # Windows
   set MINIO_ENDPOINT=127.0.0.1:9000
   set MINIO_ACCESS_KEY=minioadmin
   set MINIO_SECRET_KEY=minioadmin
   set MINIO_SECURE=false

   # Linux/Mac
   export MINIO_ENDPOINT=127.0.0.1:9000
   export MINIO_ACCESS_KEY=minioadmin
   export MINIO_SECRET_KEY=minioadmin
   export MINIO_SECURE=false
   ```

4. **Ejecutar la aplicación:**
   ```bash
   python main.py
   ```

### Reconstruir la Imagen Docker

Si realizas cambios en el código:

```bash
# Reconstruir y reiniciar
docker-compose up -d --build

# Reconstruir solo la imagen de la app
docker-compose build app
```

## 📝 API Endpoints

### Health Check
- `GET /api/health` - Verificar estado de la API y conexión con MinIO

### Buckets
- `POST /api/buckets/create` - Crear un nuevo bucket
- `GET /api/buckets/list` - Listar todos los buckets

### Archivos
- `POST /api/files/upload-single` - Subir un archivo
- `POST /api/files/upload-multiple` - Subir múltiples archivos
- `POST /api/files/download` - Descargar un archivo
- `DELETE /api/files/delete` - Eliminar un archivo
- `GET /api/files/list/{bucket_name}` - Listar archivos en un bucket

### Procesamiento PDF
- `POST /api/pdf/process-pdf-async` - Iniciar procesamiento de PDF
- `GET /api/pdf/status/{object_name}` - Obtener estado del procesamiento

### AI Prompts
- `POST /api/ai/prompt` - Configurar prompt
- `GET /api/ai/prompt` - Obtener prompt actual

## 🐛 Troubleshooting

### El contenedor no puede conectarse a MinIO

Verifica que ambos servicios estén en la misma red:
```bash
docker network inspect compress_clean_pdf-analyzer-network
```

### La base de datos no persiste

Verifica que el volumen esté montado correctamente:
```bash
docker inspect pdf-analyzer-app | grep -A 10 Mounts
```

### Permisos de la base de datos

Si hay problemas de permisos:
```bash
docker exec -it pdf-analyzer-app chmod 777 /app/src/db
```

## 📄 Licencia

MIT License
