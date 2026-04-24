# Phishing Simulation and Awareness Tool

Este proyecto es una herramienta integral de simulación de phishing diseñada para campañas de concientización corporativa. Permite enviar correos electrónicos altamente personalizados basados en la identidad corporativa de >A y realizar un seguimiento de la interacción de los usuarios en una página de aterrizaje segura.

## Caracteristicas Principales

- Servidor Web (FastAPI): Gestión de rutas para el panel de administración y la página de simulación.
- Generacion de Correos (MJML): Plantillas de correo electrónico profesionales compiladas desde MJML para máxima compatibilidad.
- Identidad Corporativa: Integración total del logo y paleta de colores de >A.
- Seguimiento en Tiempo Real: Registro de clics con marca de tiempo y dirección de correo electrónico.
- Panel de Administracion: Interfaz para subir listas de destinatarios (Excel), lanzar campañas y ver estadísticas.
- Seguimiento de Campaña: Posibilidad de enviar mensajes de seguimiento personalizados a los usuarios que interactuaron.
- Exportacion de Datos: Generación de reportes detallados en formato Excel.

## Estructura del Proyecto

La arquitectura del proyecto está organizada de forma modular:

- app/: Núcleo de la aplicación.
  - main.py: Servidor FastAPI y definición de endpoints.
  - services/: Servicios de lógica de negocio.
    - campaign_manager.py: Orquestador de la campaña de envío.
    - email_sender.py: Cliente SMTP para el envío de correos y gestión de adjuntos.
    - mjml_generator.py: Generador de plantillas MJML/HTML.
    - excel_reader.py: Procesador de archivos Excel para destinatarios.
    - token_generator.py: Generador de tokens de seguridad para los links.
- data/: Almacenamiento de archivos dinámicos (clics, listas de Excel).
- logs/: Archivos de registro de operaciones del sistema.
- static/: Activos frontend (HTML de la simulación, CSS, Logo).
- run.py: Punto de entrada principal para iniciar el servidor.
- .env: Configuración de variables de entorno (SMTP, URLs).
- requirements.txt: Dependencias de Python necesarias.

## Requisitos Previos

1. Python 3.8 o superior.
2. Node.js y MJML instalado globalmente (necesario para la compilación de correos):
   npm install -g mjml
3. Servidor SMTP (ej. Gmail con App Password) para el envío de correos.

## Instalacion y Configuracion

1. Clonar el repositorio.
2. Crear un entorno virtual e instalar las dependencias:
   pip install -r requirements.txt
3. Configurar el archivo .env con tus credenciales:
   SMTP_SERVER=smtp.gmail.com
   SMTP_PORT=587
   SENDER_EMAIL=tu_correo@ejemplo.com
   SENDER_PASSWORD=tu_contraseña_de_aplicacion
   BASE_URL=http://127.0.0.1:8888
   SECRET_KEY=tu_clave_secreta

## Uso

### Iniciar el Servidor
Ejecuta el comando desde la raíz del proyecto:
python run.py

### Acceso al Panel de Administracion
Navega a: http://0.0.0.0:8888/admin-console

### Flujo de Trabajo
1. Prepara un archivo Excel con una columna 'email'.
2. Sube el archivo en el Panel de Administración.
3. Haz clic en 'Iniciar Campaña'.
4. Los clics recibidos aparecerán automáticamente en la sección de 'Seguimiento'.
5. Puedes enviar mensajes de feedback o exportar los resultados finales a Excel antes de limpiar la base de datos.

## Documentación de APIs

A continuación se detallan los endpoints disponibles en el sistema.

### Simulación de Phishing

#### GET /token/{token}?email={email}
Registra el clic de un usuario y muestra la página de phishing.
- **Request Structure**: Parámetros en la URL (`token` y `email`).
- **Response Structure**: Archivo HTML (`index.html`).

---

### Administración de Campañas

#### GET /admin-console
Acceso al panel de administración.
- **Request Structure**: N/A.
- **Response Structure**: Archivo HTML (`admin.html`).

#### POST /admin/upload-excel
Sube una lista de destinatarios en formato Excel.
- **Request Structure**: `multipart/form-data` con campo `file`.
- **Response Structure**:
  ```json
  {
    "status": "success",
    "filename": "nombre_archivo.xlsx"
  }
  ```

#### POST /admin/send-campaign
Inicia el envío masivo de correos de la campaña actual.
- **Request Structure**: N/A.
- **Response Structure**:
  ```json
  {
    "status": "success",
    "stats": {
      "total": 10,
      "enviados": 10,
      "fallos": 0,
      "errores": []
    }
  }
  ```

#### POST /admin/send-followup
Envía un mensaje de seguimiento a los usuarios capturados.
- **Request Structure**:
  ```json
  {
    "message": "Texto del mensaje de seguimiento"
  }
  ```
- **Response Structure**:
  ```json
  {
    "status": "success",
    "sent": 5,
    "total": 5
  }
  ```

#### GET /admin/stats
Obtiene el listado de clics registrados.
- **Request Structure**: N/A.
- **Response Structure**:
  ```json
  [
    {
      "email": "ejemplo@a.mx",
      "timestamp": "2026-03-11 10:00:00"
    }
  ]
  ```

#### GET /admin/export-results
Descarga un archivo Excel con los resultados.
- **Request Structure**: N/A.
- **Response Structure**: Archivo binario (.xlsx).

#### POST /admin/reset-stats
Borra los registros de clics y listas de correos cargadas.
- **Request Structure**: N/A.
- **Response Structure**:
  ```json
  {
    "status": "success",
    "message": "Estadísticas borradas correctamente."
  }
  ```

#### GET /admin/download-template
Descarga una plantilla de Excel lista para ser completada.
- **Request Structure**: N/A.
- **Response Structure**: Archivo binario (.xlsx).

---

## Aviso Legal y Etico
Esta herramienta ha sido creada exclusivamente con fines educativos y de concientización de seguridad informática. El uso de este software para actividades maliciosas es estrictamente ilegal y está prohibido. El autor no se hace responsable por el mal uso de esta herramienta.
