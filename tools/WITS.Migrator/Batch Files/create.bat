@echo off
if "%~1"=="" (
    echo Error: Please specify migration name
    echo Usage: create.bat <migration_name>
    pause
    exit /b 1
)

echo Creating migration %1...
dotnet run --project "%~dp0WITS.DbManager.csproj" create %1
if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to create migration
    pause
    exit /b %ERRORLEVEL%
)
pause
