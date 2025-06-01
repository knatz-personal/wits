@echo off
if "%~1"=="" (
    echo Error: Please specify migration ID to remove, or use 'all' to remove all pending migrations
    echo Usage: remove.bat ^<migration_id^|all^>
    pause
    exit /b 1
)

if /i "%~1"=="all" (
    echo Removing all pending migrations...
) else (
    echo Removing migration "%~1"...
)

dotnet run --project "%~dp0\WITS.DbManager.csproj" remove "%~1"
if errorlevel 1 (
    echo Error: Failed to remove migration
    pause
    exit /b 1
)
pause
