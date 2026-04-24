# main.py - VERSIÓN COMPLETA Y LIMPIA

from fastapi import FastAPI, File, UploadFile, HTTPException, Form
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse
from typing import List, Optional
import tempfile
import os
import uuid
from datetime import datetime
from pydantic import BaseModel
import io
import asyncio
from concurrent.futures import ThreadPoolExecutor
import time

from src.db.database import (
    create_db, 
    insert_file_status, 
    insert_need_modify,
    insert_split_info,
    update_file_status,
    update_need_modify, 
    get_file_status,
    get_need_modify,
    get_split_info,
    delete_need_modify, 
    insert_prompt,
    select_prompt,
    update_prompt
)
from src.services.analyzeServices import only_check_file, process_file
from src.models.models import (
    PdfProcessingRequest, 
    PdfProcessingResponse, 
    AIPrompt
)
from src.minio.minioServices import (
    get_minio_client,
    create_bucket,
    upload_file,
    download_file,
    delete_file
)

from io import BytesIO

app = FastAPI(title="PDF Analyzer API", version="1.0.0")
executor = ThreadPoolExecutor(max_workers=3)

# CORS Configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic Models
class BucketCreate(BaseModel):
    bucket_name: str

class FileDelete(BaseModel):
    object_name: str
    bucket_name: str

class FileDownload(BaseModel):
    object_name: str
    bucket_name: str

# Configuration
DEFAULT_BUCKET = "pdf-uploads"
MODIFIED_BUCKET = "pdf-modified"

# ============================================
# STARTUP
# ============================================

@app.on_event("startup")
async def startup_event():
    """Initialize database and create default buckets"""
    await create_db()
    client = get_minio_client()
    create_bucket(client, DEFAULT_BUCKET)
    create_bucket(client, MODIFIED_BUCKET)
    print("✅ Startup complete - DB and buckets ready")

# ============================================
# BUCKET ENDPOINTS
# ============================================

@app.post("/api/buckets/create")
async def create_bucket_endpoint(bucket_data: BucketCreate):
    """Create a new bucket"""
    try:
        client = get_minio_client()
        create_bucket(client, bucket_data.bucket_name)
        return {
            "success": True,
            "message": f"Bucket '{bucket_data.bucket_name}' created successfully",
            "bucket_name": bucket_data.bucket_name
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error creating bucket: {str(e)}")

@app.get("/api/buckets/list")
async def list_buckets():
    """List all available buckets"""
    try:
        client = get_minio_client()
        buckets = client.list_buckets()
        bucket_list = [{"name": bucket.name, "creation_date": bucket.creation_date} for bucket in buckets]
        return {
            "success": True,
            "buckets": bucket_list
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error listing buckets: {str(e)}")

# ============================================
# FILE UPLOAD ENDPOINTS
# ============================================

@app.post("/api/files/upload-multiple")
async def upload_multiple_files(
    files: List[UploadFile] = File(...),
    bucket_name: Optional[str] = Form(DEFAULT_BUCKET)
):
    """Upload multiple PDF files"""
    if not files:
        raise HTTPException(status_code=400, detail="No files provided")
    
    uploaded_files = []
    failed_files = []
    client = get_minio_client()
    create_bucket(client, bucket_name)
    
    for file in files:
        temp_path = None
        try:
            if not file.filename.lower().endswith('.pdf'):
                failed_files.append({
                    "filename": file.filename,
                    "error": "Only PDF files are allowed"
                })
                continue
            
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            unique_id = str(uuid.uuid4())[:8]
            object_name = f"{timestamp}_{unique_id}_{file.filename}"
            
            temp_path = None
            try:
                with tempfile.NamedTemporaryFile(delete=False, suffix=".pdf") as temp_file:
                    temp_path = temp_file.name
                    content = await file.read()
                    temp_file.write(content)
                    temp_file.flush()
                
                # Upload to MinIO
                await upload_file(client, temp_path, object_name, bucket_name)
                
                # Insert to DB
                db_result = await insert_file_status(
                    filename=file.filename,
                    object_name=object_name,
                    bucket_name=bucket_name,
                    size_bytes=len(content)
                )
                
                if db_result:
                    print(f"[Upload] ✅ {file.filename} uploaded and registered")
                    uploaded_files.append({
                        "original_filename": file.filename,
                        "object_name": object_name,
                        "bucket_name": bucket_name,
                        "size_bytes": len(content),
                        "upload_timestamp": datetime.now().isoformat()
                    })
                else:
                    print(f"[Upload] ⚠️ {file.filename} uploaded but DB insert failed")
                    
            finally:
                if temp_path and os.path.exists(temp_path):
                    try:
                        time.sleep(0.1)
                        os.unlink(temp_path)
                    except Exception as e:
                        print(f"[Upload] Could not delete temp: {e}")

        except Exception as e:
            print(f"[Upload] ❌ Error with {file.filename}: {e}")
            failed_files.append({
                "filename": file.filename,
                "error": str(e)
            })
    
    return {
        "success": True,
        "message": f"Processed {len(files)} files",
        "uploaded_files": uploaded_files,
        "failed_files": failed_files,
        "total_uploaded": len(uploaded_files),
        "total_failed": len(failed_files)
    }

@app.post("/api/files/upload-single")
async def upload_single_file(
    file: UploadFile = File(...),
    bucket_name: Optional[str] = Form(DEFAULT_BUCKET),
    custom_name: Optional[str] = Form(None)
):
    """Upload a single file"""
    temp_path = None
    try:
        client = get_minio_client()
        create_bucket(client, bucket_name)
        
        if custom_name:
            object_name = custom_name
        else:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            unique_id = str(uuid.uuid4())[:8]
            object_name = f"{timestamp}_{unique_id}_{file.filename}"
        
        try:
            with tempfile.NamedTemporaryFile(delete=False, suffix=".pdf") as temp_file:
                temp_path = temp_file.name
                content = await file.read()
                temp_file.write(content)
                temp_file.flush()
            
            await upload_file(client, temp_path, object_name, bucket_name)
            
            return {
                "success": True,
                "message": "File uploaded successfully",
                "original_filename": file.filename,
                "object_name": object_name,
                "bucket_name": bucket_name,
                "size_bytes": len(content)
            }
        finally:
            if temp_path and os.path.exists(temp_path):
                try:
                    time.sleep(0.1)
                    os.unlink(temp_path)
                except Exception as e:
                    print(f"Could not delete temp file: {e}")
            
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error uploading file: {str(e)}")

# ============================================
# FILE DOWNLOAD ENDPOINTS
# ============================================

@app.post("/api/files/download")
async def download_file_endpoint(file_data: FileDownload):
    """Download a file from MinIO"""
    try:
        client = get_minio_client()
        
        try:
            client.stat_object(file_data.bucket_name, file_data.object_name)
        except:
            raise HTTPException(status_code=404, detail="File not found")
        
        temp_file = tempfile.NamedTemporaryFile(delete=False, suffix=".pdf")
        temp_file.close()
        
        download_file(client, file_data.object_name, temp_file.name, file_data.bucket_name)
        
        return FileResponse(
            path=temp_file.name,
            filename=file_data.object_name,
            media_type='application/octet-stream'
        )
        
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error downloading file: {str(e)}")

# ============================================
# FILE DELETE ENDPOINTS
# ============================================

@app.delete("/api/files/delete")
async def delete_file_endpoint(file_data: FileDelete):
    """Delete a file from MinIO"""
    try:
        client = get_minio_client()
        delete_file(client, file_data.object_name, file_data.bucket_name)
        
        return {
            "success": True,
            "message": f"File deleted successfully",
            "object_name": file_data.object_name,
            "bucket_name": file_data.bucket_name
        }
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error deleting file: {str(e)}")

# ============================================
# FILE LIST ENDPOINTS
# ============================================

@app.get("/api/files/list/{bucket_name}")
async def list_files(bucket_name: str):
    """List all files in a bucket with full status info"""
    try:
        client = get_minio_client()
        
        if not client.bucket_exists(bucket_name):
            raise HTTPException(status_code=404, detail="Bucket not found")
        
        objects = client.list_objects(bucket_name)
        file_list = []
        
        for obj in objects:
            # Get DB status
            data = await get_file_status(objectname=obj.object_name)
            
            if not data:
                print(f"[ListFiles] ⚠️ {obj.object_name} not found in DB")
                continue
            
            # Parse DB tuple: (id, filename, object_name, bucket_name, size_bytes, status, has_modified, created_at, updated_at)
            db_filename = data[1]
            db_status = data[5]
            db_has_modified = data[6]
            
            # Get split info if exists
            split_info = await get_split_info(obj.object_name, MODIFIED_BUCKET)
            
            file_info = {
                "file_name": db_filename,
                "object_name": obj.object_name,
                "size_bytes": obj.size,
                "status": db_status,
                "has_modified": db_has_modified,
                "is_split": False,
                "part1_name": None,
                "part2_name": None,
                "last_modified": obj.last_modified.isoformat() if obj.last_modified else None,
                "etag": obj.etag
            }
            
            # Add split info if available
            if split_info:
                file_info["is_split"] = True
                file_info["part1_name"] = split_info[0]
                file_info["part2_name"] = split_info[1]
            
            file_list.append(file_info)
        
        return {
            "success": True,
            "bucket_name": bucket_name,
            "files": file_list,
            "total_files": len(file_list)
        }
        
    except HTTPException:
        raise
    except Exception as e:
        print(f"[ListFiles] ❌ Error: {e}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=f"Error listing files: {str(e)}")

@app.get("/api/files/info/{bucket_name}/{object_name}")
async def get_file_info(bucket_name: str, object_name: str):
    """Get file info"""
    try:
        client = get_minio_client()
        
        try:
            stat = client.stat_object(bucket_name, object_name)
            return {
                "success": True,
                "object_name": object_name,
                "bucket_name": bucket_name,
                "size_bytes": stat.size,
                "last_modified": stat.last_modified.isoformat() if stat.last_modified else None,
                "etag": stat.etag,
                "content_type": stat.content_type
            }
        except:
            raise HTTPException(status_code=404, detail="File not found")
            
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error getting file info: {str(e)}")

# ============================================
# HEALTH CHECK
# ============================================

@app.get("/api/health")
async def health_check():
    """Health check"""
    try:
        client = get_minio_client()
        buckets = list(client.list_buckets())
        return {
            "success": True,
            "message": "API is healthy",
            "minio_connection": "OK",
            "buckets_count": len(buckets)
        }
    except Exception as e:
        return {
            "success": False,
            "message": "API is running but MinIO connection failed",
            "error": str(e)
        }

# ============================================
# PDF PROCESSING ENDPOINTS
# ============================================

@app.post("/api/pdf/process-pdf-async")
async def process_pdf_async(request: PdfProcessingRequest):
    """Start async PDF processing"""
    try:
        await update_file_status(
            filename=request.filename, 
            status="analyzing", 
            object=request.object_name, 
            bucket=request.bucket_name
        )
        
        asyncio.create_task(process_pdf_background(request))
        
        return {
            "success": True,
            "message": "Processing started",
            "status": "analyzing"
        }
    except Exception as e:
        await update_file_status(
            filename=request.filename, 
            status="error", 
            object=request.object_name, 
            bucket=request.bucket_name
        )
        return {
            "success": False,
            "error": str(e)
        }

async def process_pdf_background(request: PdfProcessingRequest):
    """Background PDF processing with split support"""
    temp_path = None
    part1_path = None
    part2_path = None
    
    try:
        client = get_minio_client()

        try:
            file = client.get_object(request.bucket_name, request.object_name)
        except:
            await update_file_status(
                filename=request.filename, 
                status="error", 
                object=request.object_name, 
                bucket=request.bucket_name
            )
            return
                
        temp_path = f"tmp_{uuid.uuid4()}_{request.filename}"
        file_content = file.read()
        
        with open(temp_path, "wb") as f:
            f.write(file_content)

        print(f"[ProcessPDF] Processing: {request.filename}")

        # Process the file
        result = await process_file(temp_path)

        # Check if file was split
        if isinstance(result, dict) and result.get('type') == 'split':
            print(f"[ProcessPDF] File split into 2 parts")
            
            part1_path = result['part1']
            part2_path = result['part2']
            
            try:
                part1_obj_name = f"{request.object_name.replace('.pdf', '')}_part1.pdf"
                part2_obj_name = f"{request.object_name.replace('.pdf', '')}_part2.pdf"
                
                # Upload part 1
                client = get_minio_client()
                create_bucket(client, MODIFIED_BUCKET)
                await upload_file(client, part1_path, part1_obj_name, MODIFIED_BUCKET)
                
                # Upload part 2
                await upload_file(client, part2_path, part2_obj_name, MODIFIED_BUCKET)
                
                # Register split info
                await insert_split_info(
                    original_object=request.object_name,
                    part1_name=part1_obj_name,
                    part2_name=part2_obj_name,
                    bucket_name=MODIFIED_BUCKET
                )
                
                # Update status
                await update_file_status(
                    filename=request.filename, 
                    status="split", 
                    modified=1, 
                    object=request.object_name, 
                    bucket=request.bucket_name
                )
                
                await insert_need_modify(
                    original_file=request.object_name,
                    filename=request.filename,
                    object_name=request.object_name,
                    bucket_name=request.bucket_name,
                    status="completed",
                    size_bytes=len(file_content)
                )
                
                print(f"[ProcessPDF] ✅ Split and uploaded successfully")
                
            except Exception as e:
                print(f"[ProcessPDF] ❌ Upload error: {e}")
                await update_file_status(
                    filename=request.filename, 
                    status="error", 
                    object=request.object_name, 
                    bucket=request.bucket_name
                )
        
        elif result is True:
            print(f"[ProcessPDF] File processed successfully (no split)")

            try:
                # Upload the processed file to MinIO
                client = get_minio_client()
                create_bucket(client, MODIFIED_BUCKET)
                await upload_file(client, temp_path, request.object_name, MODIFIED_BUCKET)
                    
                await update_file_status(
                    filename=request.filename, 
                    status="fixed", 
                    modified=1, 
                    object=request.object_name, 
                    bucket=request.bucket_name
                )
                await delete_need_modify(object=request.object_name, bucket=request.bucket_name)
                
            except Exception as e:
                print(f"[ProcessPDF] ❌ Upload error: {e}")
                await update_file_status(
                    filename=request.filename, 
                    status="error", 
                    object=request.object_name, 
                    bucket=request.bucket_name
                )
        else:
            print("[ProcessPDF] No processing needed")
            await update_file_status(
                filename=request.filename, 
                status="no_changes", 
                object=request.object_name, 
                bucket=request.bucket_name
            )

    except Exception as e:
        print(f"[ProcessPDF] ❌ Error: {e}")
        import traceback
        traceback.print_exc()
        
        await update_file_status(
            filename=request.filename, 
            status="error", 
            object=request.object_name, 
            bucket=request.bucket_name
        )
    
    finally:
        # Cleanup
        for path in [temp_path, part1_path, part2_path]:
            if path and os.path.exists(path):
                try:
                    os.remove(path)
                except:
                    pass

# ============================================
# STATUS ENDPOINT
# ============================================

@app.get("/api/pdf/status/{object_name}")
async def get_processing_status(object_name: str, bucket_name: str = DEFAULT_BUCKET):
    """Get file processing status"""
    try:
        data = await get_file_status(objectname=object_name)
        
        if data:
            status = data[5]
            has_modified = data[6]
            
            # Get split info
            split_info = await get_split_info(object_name, MODIFIED_BUCKET)
            
            response = {
                "success": True,
                "status": status,
                "has_modified": has_modified,
                "is_split": False,
                "part1_name": None,
                "part2_name": None
            }
            
            if split_info:
                response["is_split"] = True
                response["part1_name"] = split_info[0]
                response["part2_name"] = split_info[1]
            
            return response
        else:
            return {
                "success": False,
                "error": "File not found in database"
            }
    except Exception as e:
        return {
            "success": False,
            "error": str(e)
        }

# ============================================
# AI PROMPT ENDPOINTS
# ============================================

@app.post("/api/ai/prompt")
async def set_prompt(prompt: AIPrompt):
    try:
        if len(prompt.prompt_text) <= 0:
            raise HTTPException(status_code=404, detail="Text not provided")
        
        result = await select_prompt()

        if result is None:
            await insert_prompt(prompt=prompt.prompt_text)
        else:
            await update_prompt(prompt=prompt.prompt_text)

        new_prompt = await select_prompt()
        return {"prompt": new_prompt[1] if new_prompt else None}

    except Exception as e:
        return {"success": False, "error": str(e)}
    
@app.get("/api/ai/prompt")
async def get_prompt():
    try:
        result = await select_prompt()
        if result is None:
            return {"prompt": None}
        return {"prompt": result[1]}
    except Exception as e:
        return {"success": False, "error": str(e)}
    
@app.post("/api/ai/prompt-enhance")
async def enhance_prompt(prompt: AIPrompt):
    try:
        return {"prompt": "enhanced api"}
    except Exception as e:
        return {"success": False, "error": str(e)}

# ============================================
# MAIN
# ============================================

if __name__ == "__main__":
    import uvicorn
    import asyncio
    asyncio.run(create_db())
    uvicorn.run(app, host="0.0.0.0", port=8011)