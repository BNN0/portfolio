from minio import Minio
from minio.error import S3Error

def get_minio_client():
    return Minio(
        "127.0.0.1:9000",  # Cambia esto por tu endpoint de MinIO
        access_key="minioadmin",
        secret_key="minioadmin",
        secure=False
    )

def download_file(client, object_name, file_path, bucket_name):
    try:
        client.fget_object(bucket_name, object_name, file_path)
        print(f"File '{object_name}' downloaded from '{bucket_name}'.")
    except S3Error as exc:
        print("Error occurred while downloading the file:", exc)