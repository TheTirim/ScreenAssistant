# TabZero Assistant (MVP)

Windows-only desktop assistant MVP with a WPF tray app and local FastAPI service.

## Prerequisites

- Windows 10/11
- .NET 8 SDK
- Python 3.11+

## Run the WPF App

```powershell
# from repo root
cd src/TabZeroAssistant.Wpf

dotnet run
```

The WPF app will:
- create `%APPDATA%\TabZeroAssistant\assistant.db`
- create `%APPDATA%\TabZeroAssistant\masterkey.protected`
- start the local Python service if it is not running

## Run the Python Service Only

```powershell
cd python_service
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python -m uvicorn app:app --host 127.0.0.1 --port 8123
```

## Dev Convenience Script

```powershell
.\scripts\dev-run.ps1
```

## Data Storage

All data is stored per Windows user:

- Database: `%APPDATA%\TabZeroAssistant\assistant.db`
- Protected master key: `%APPDATA%\TabZeroAssistant\masterkey.protected`
- Settings: `%APPDATA%\TabZeroAssistant\settings.json`

## Notes

- No cloud calls; everything runs locally.
- UI strings are localized based on `CurrentUICulture` (de-DE for German, otherwise English).
- Actions are suggestions only; execution requires an explicit click.

See `docs/SECURITY.md` and `docs/THREAT_MODEL.md` for details.
