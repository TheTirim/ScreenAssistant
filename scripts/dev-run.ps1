$ErrorActionPreference = "Stop"

$serviceDir = Join-Path $PSScriptRoot "..\python_service"
Set-Location $serviceDir

if (-not (Test-Path ".venv")) {
    python -m venv .venv
}

.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python -m uvicorn app:app --host 127.0.0.1 --port 8123
