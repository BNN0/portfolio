from __future__ import annotations

from ..config import settings


def create_s3_client():
    try:
        import boto3
    except ModuleNotFoundError as e:
        raise ModuleNotFoundError(
            "Missing dependency 'boto3'. Install requirements: pip install -r requirements.txt"
        ) from e

    return boto3.client(
        "s3",
        endpoint_url=settings.minio_endpoint,
        aws_access_key_id=settings.minio_access_key,
        aws_secret_access_key=settings.minio_secret_key,
    )


def ensure_bucket_exists(s3_client, bucket_name: str) -> None:
    try:
        from botocore.exceptions import ClientError
    except ModuleNotFoundError as e:
        raise ModuleNotFoundError(
            "Missing dependency 'botocore'. Install requirements: pip install -r requirements.txt"
        ) from e

    try:
        s3_client.head_bucket(Bucket=bucket_name)
        return
    except ClientError as e:
        error_code = e.response.get("Error", {}).get("Code")
        if str(error_code) != "404":
            return

    try:
        s3_client.create_bucket(Bucket=bucket_name)
    except Exception:
        # Bucket may already exist or user lacks perms; don't crash the app at startup.
        pass

