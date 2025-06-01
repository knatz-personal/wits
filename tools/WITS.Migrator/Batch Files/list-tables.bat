@echo off
echo Listing database tables...
dotnet run --project "%~dp0\WITS.DbManager.csproj" list-tables
if errorlevel 1 (
    echo Error: Failed to list tables
    pause
    exit /b 1
)
pause
