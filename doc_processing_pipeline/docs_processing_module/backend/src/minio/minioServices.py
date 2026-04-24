from minio import Minio
from minio.error import S3Error
import io
import os

# Inicialización del cliente MinIO
def get_minio_client():
    """
    Get MinIO client with configuration from environment variables.
    
    Environment variables:
    - MINIO_ENDPOINT: MinIO server endpoint (default: 127.0.0.1:9000)
    - MINIO_ACCESS_KEY: Access key (default: minioadmin)
    - MINIO_SECRET_KEY: Secret key (default: minioadmin)
    - MINIO_SECURE: Use HTTPS (default: false)
    """
    endpoint = os.getenv("MINIO_ENDPOINT", "127.0.0.1:9000")
    access_key = os.getenv("MINIO_ACCESS_KEY", "minioadmin")
    secret_key = os.getenv("MINIO_SECRET_KEY", "minioadmin")
    secure = os.getenv("MINIO_SECURE", "false").lower() == "true"
    
    return Minio(
        endpoint,
        access_key=access_key,
        secret_key=secret_key,
        secure=secure
    )

# Creación de un bucket (equivalente a una carpeta)
def create_bucket(client, bucket_name):
    try:
        if not client.bucket_exists(bucket_name):
            client.make_bucket(bucket_name)
            print(f"Bucket '{bucket_name}' created successfully.")
        else:
            print(f"Bucket '{bucket_name}' already exists.")
    except S3Error as exc:
        print("Error occurred while creating bucket:", exc)

# Subida de un archivo a un bucket
async def upload_file(client, file_path_or_bytes, object_name, bucket_name):
    """
    Subir un archivo al bucket. Puede manejar tanto archivos en disco como datos en memoria.

    Args:
        client: Cliente de MinIO.
        file_path_or_bytes: Ruta al archivo o datos en memoria (bytes).
        object_name: Nombre del objeto en el bucket.
        bucket_name: Nombre del bucket.
    """
    try:
        if isinstance(file_path_or_bytes, bytes):
            # Subir datos en memoria
            client.put_object(
                bucket_name,
                object_name,
                io.BytesIO(file_path_or_bytes),
                len(file_path_or_bytes)
            )
            print(f"Bytes uploaded to '{bucket_name}/{object_name}'.")
        else:
            # Subir archivo desde disco
            client.fput_object(bucket_name, object_name, file_path_or_bytes)
            print(f"File '{file_path_or_bytes}' uploaded to '{bucket_name}/{object_name}'.")
    except S3Error as exc:
        print("Error occurred while uploading the file:", exc)

# Descarga de un archivo desde un bucket
def download_file(client, object_name, file_path, bucket_name):
    try:
        client.fget_object(bucket_name, object_name, file_path)
        print(f"File '{object_name}' downloaded from '{bucket_name}'.")
    except S3Error as exc:
        print("Error occurred while downloading the file:", exc)

# Eliminación de un archivo de un bucket
def delete_file(client, object_name, bucket_name):
    try:
        client.remove_object(bucket_name, object_name)
        print(f"File '{object_name}' deleted from '{bucket_name}'.")
    except S3Error as exc:
        print("Error occurred while deleting the file:", exc)

# Ejemplo de uso
if __name__ == "__main__":
    client = get_minio_client()

    bucket_name = "my-bucket"
    
    # Crear un bucket
    create_bucket(client, bucket_name)
    
    # Subir archivo
    upload_file(client, "requirements.txt", "requirements.txt", bucket_name)

    # Descargar archivo
    download_file(client, "requirements.txt", "requirements.txt", bucket_name)
    
    # Eliminar archivo
    delete_file(client, "requirements.txt", bucket_name)
