import os
from dataclasses import dataclass

from dotenv import load_dotenv


load_dotenv()


@dataclass(frozen=True)
class Settings:
    # App
    title: str = "DocExtractor App"

    # MinIO/S3
    minio_endpoint: str = os.environ.get("MINIO_ENDPOINT", "http://localhost:9000")
    minio_access_key: str = os.environ.get("MINIO_ACCESS_KEY", "minioadmin")
    minio_secret_key: str = os.environ.get("MINIO_SECRET_KEY", "minioadmin")
    minio_bucket_name: str = os.environ.get("MINIO_BUCKET_NAME", "documents")

    # LLM
    gemini_model: str = os.getenv("GEMINI_MODEL", "gemini-2.5-flash")


settings = Settings()

