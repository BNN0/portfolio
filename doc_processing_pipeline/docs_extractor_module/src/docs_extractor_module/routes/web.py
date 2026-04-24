from __future__ import annotations

from pathlib import Path

from fastapi import APIRouter
from fastapi.responses import FileResponse


def create_web_router(*, web_root: Path) -> APIRouter:
    router = APIRouter()

    def _file(name: str) -> FileResponse:
        return FileResponse(str(web_root / name))

    @router.get("/")
    async def root():
        return _file("index.html")

    @router.get("/processing.html")
    async def processing():
        return _file("processing.html")

    @router.get("/edit.html")
    async def edit():
        return _file("edit.html")

    @router.get("/download.html")
    async def download():
        return _file("download.html")

    @router.get("/style.css")
    async def style():
        return _file("style.css")

    return router

