@echo off
echo Running migrations...
dotnet run --project "%~dp0WITS.DbManager.csproj" migrate
if %ERRORLEVEL% NEQ 0 (
    echo Error: Migration failed
    pause
    exit /b %ERRORLEVEL%
)
pause
