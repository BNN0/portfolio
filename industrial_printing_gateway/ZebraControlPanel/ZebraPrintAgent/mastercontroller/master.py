import subprocess
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
import requests

app = FastAPI()

origins = ["*"]

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/health")
def health():
    return {"status": "running"}

@app.post("/start")
def start_service():
    result = subprocess.run(
        [r"C:\\ProgramData\\chocolatey\\lib\\NSSM\\tools\\nssm.exe", "start", "zebra-api"],
        shell=True, capture_output=True, text=True
    )
    if result.returncode != 0:
        raise HTTPException(status_code=500, detail=f"Failed to start service: {result.stderr.strip()}")
    return {"status": "started"}

@app.post("/stop")
def stop_service():
    result = subprocess.run(
        [r"C:\\ProgramData\\chocolatey\\lib\\NSSM\\tools\\nssm.exe", "stop", "zebra-api"],
        shell=True, capture_output=True, text=True
    )
    if result.returncode != 0:
        raise HTTPException(status_code=500, detail=f"Failed to stop service: {result.stderr.strip()}")
    return {"status": "stopped"}

@app.post("/restart")
def restart_service():
    result = subprocess.run(
        [r"C:\\ProgramData\\chocolatey\\lib\\NSSM\\tools\\nssm.exe", "restart", "zebra-api"],
        shell=True, capture_output=True, text=True
    )
    if result.returncode != 0:
        raise HTTPException(status_code=500, detail=f"Failed to restart service: {result.stderr.strip()}")
    return {"status": "restarted"}

@app.get("/status")
def status_service():
    result = subprocess.run(["nssm", "status", "zebra-api"], shell=True, capture_output=True, text=True)
    if result.returncode != 0:
        raise HTTPException(status_code=500, detail="Failed to get service status")
    return {"status": result.stdout.strip()}

@app.get("/backend-address")
def backend_address():
    try:
        response = requests.get("http://localhost:8090/get-ip/")
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        raise HTTPException(status_code=500, detail=f"Failed to fetch backend address: {str(e)}")