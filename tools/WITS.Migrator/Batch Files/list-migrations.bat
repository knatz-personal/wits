@echo off
echo Listing migrations...
dotnet run --project "%~dp0\WITS.DbManager.csproj" list-migrations
if errorlevel 1 (
    echo Error: Failed to list migrations
    pause
    exit /b 1
)
pause
