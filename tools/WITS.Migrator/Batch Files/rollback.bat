@echo off
if "%~1"=="" (
    echo Error: Please specify migration ID to rollback
    echo Usage: rollback.bat <migration_id>
    pause
    exit /b 1
)

echo Rolling back migration %1...
dotnet run --project "%~dp0WITS.DbManager.csproj" rollback %1
if %ERRORLEVEL% NEQ 0 (
    echo Error: Rollback failed
    pause
    exit /b %ERRORLEVEL%
)
pause
