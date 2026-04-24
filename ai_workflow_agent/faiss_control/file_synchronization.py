import boto3
import os
import datetime

from dotenv import load_dotenv
load_dotenv()

from aws_config.aws import initialize_s3bucket

# Define the path for temporary storage
folder_path = os.getenv("FAISS_FOLDER_PATH")

# Ensure the temporary directory exists
os.makedirs(folder_path, exist_ok=True)
BUCKET_NAME = "bucket-chatbot-trantor-test"

def download_file():
    s3_client = initialize_s3bucket()
    response = s3_client.list_objects_v2(Bucket=BUCKET_NAME)
    print(f"Archivos en el bucket {BUCKET_NAME}")
    s3_file_keys = set()
    if 'Contents' in response:
        for obj in response['Contents']:
            file_key = obj['Key']
            
            # Filtrar solo archivos con extensión .faiss y .pkl
            if file_key.endswith('.faiss') or file_key.endswith('.pkl'):
                s3_file_keys.add(file_key)
                s3_last_modified = obj['LastModified']
                local_filename = os.path.join(folder_path, file_key)
                
                if os.path.exists(local_filename):
                    # Obtener la fecha de última modificación del archivo local
                    local_last_modified_time = os.path.getmtime(local_filename)
                    local_last_modified = datetime.datetime.fromtimestamp(
                        local_last_modified_time, tz=s3_last_modified.tzinfo
                    )
                    # Comparar las fechas de modificación
                    if s3_last_modified > local_last_modified:
                        # El archivo en S3 es más reciente, eliminar y descargar
                        os.remove(local_filename)
                        os.makedirs(os.path.dirname(local_filename), exist_ok=True)
                        s3_client.download_file(
                            Bucket=BUCKET_NAME, Key=file_key, Filename=local_filename
                        )
                        print(f"Archivo actualizado y descargado: {local_filename}")
                    else:
                        print(f"El archivo local está actualizado: {local_filename}")
                else:
                    # El archivo local no existe, descargar
                    os.makedirs(os.path.dirname(local_filename), exist_ok=True)
                    s3_client.download_file(
                        Bucket=BUCKET_NAME, Key=file_key, Filename=local_filename
                    )
                    print(f"Archivo descargado: {local_filename}")
    else:
        print("No se encontraron archivos en el bucket.")

    # Ahora maneja los archivos locales que no están en S3
    local_files = set()
    for root, dirs, files in os.walk(folder_path):
        for file in files:
            # Obtener la ruta relativa del archivo con respecto a folder_path
            relative_path = os.path.relpath(os.path.join(root, file), folder_path)
            local_files.add(relative_path)

    # Identificar los archivos locales que no están en S3
    files_to_delete = local_files - s3_file_keys

    for file_key in files_to_delete:
        local_filename = os.path.join(folder_path, file_key)
        os.remove(local_filename)
        print(f"Archivo local eliminado: {local_filename}")