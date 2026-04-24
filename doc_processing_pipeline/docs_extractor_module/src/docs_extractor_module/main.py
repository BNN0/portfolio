from __future__ import annotations

from pathlib import Path

from fastapi import FastAPI

from .config import settings
from .routes.api import create_api_router
from .routes.web import create_web_router


def create_app() -> FastAPI:
    app = FastAPI(title=settings.title)

    web_root = Path(__file__).parent / "web"

    # Explicit routes that return the same pages as before
    app.include_router(create_web_router(web_root=web_root))
    app.include_router(create_api_router())

    return app


app = create_app()

