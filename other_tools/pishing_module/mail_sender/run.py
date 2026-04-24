import uvicorn
import os
import sys

# Añadir el directorio actual al path para que reconozca el paquete 'app'
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

if __name__ == "__main__":
    uvicorn.run("app.main:app", host="127.0.0.1", port=8888, reload=True)
