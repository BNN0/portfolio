from __future__ import annotations

from typing import Any, Dict

# Keys are filenames, values are progress integers (0-100) or strings
PROCESSING_STATUS: Dict[str, Any] = {}

# Keys are filenames, values are the extracted JSON dict
PROCESSING_RESULTS: Dict[str, Dict[str, Any]] = {}

