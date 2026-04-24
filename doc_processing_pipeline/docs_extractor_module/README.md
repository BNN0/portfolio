# docs_extractor_module

Backend (FastAPI) + UI estática para subir documentos, extraer información con Gemini (LangChain) y exportar a Excel.

## Estructura

- `src/docs_extractor_module/`: paquete Python
  - `main.py`: crea la app (`create_app`) y registra rutas
  - `routes/`: endpoints web y API
  - `services/`: lógica de extracción + MinIO/S3
  - `models.py`: modelos Pydantic + prompt del extractor
  - `web/`: `index.html`, `processing.html`, `edit.html`, `download.html`, `style.css`
- `app.py`: entrypoint compatible (para `python app.py`)
- `requirements.txt`: dependencias

## Ejecutar

### Opción A (recomendada)

```bash
pip install -r requirements.txt
uvicorn docs_extractor_module.main:app --reload --port 8884
```

> Si estás en Windows, asegúrate de tener `PYTHONPATH=src` o ejecutar desde la raíz del módulo.

### Opción B (compat)

```bash
pip install -r requirements.txt
python app.py
```

## Variables de entorno

Crea un `.env` (ver `.env.example`).

