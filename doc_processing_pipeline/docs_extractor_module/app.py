"""
Backward-compatible entrypoint.

Prefer running:
  uvicorn docs_extractor_module.main:app --reload --port 8884
"""

import sys
from pathlib import Path

# Ensure `src/` is on sys.path when running `python app.py`
ROOT = Path(__file__).resolve().parent
SRC = ROOT / "src"
if str(SRC) not in sys.path:
    sys.path.insert(0, str(SRC))

from docs_extractor_module.main import app  # noqa: E402  (import after sys.path tweak)


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8884)

