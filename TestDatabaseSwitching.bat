@echo off
echo ========================================
echo Testing Database Provider Switching
echo ========================================

echo.
echo 1. Testing SQLite Configuration...
echo Setting DatabaseProvider to Sqlite
echo DatabaseProvider=Sqlite > .env.temp

echo.
echo 2. Building Application...
dotnet build --verbosity quiet
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    exit /b 1
)

echo.
echo 3. Checking SQLite Database File...
if exist "POS_Database.db" (
    echo ✅ SQLite database file exists: POS_Database.db
    dir POS_Database.db | findstr POS_Database.db
) else (
    echo ❌ SQLite database file not found
)

echo.
echo 4. Testing Application Startup with SQLite...
timeout /t 2 >nul
echo Starting application (will stop after 10 seconds)...
start /b dotnet run --no-build > startup.log 2>&1
timeout /t 10 >nul
taskkill /f /im dotnet.exe >nul 2>&1

echo.
echo 5. Checking Startup Logs...
if exist "startup.log" (
    findstr "Using database provider" startup.log
    findstr "Database migration completed" startup.log
    del startup.log
)

echo.
echo 6. Switching to SQL Server Configuration...
echo Setting DatabaseProvider to SqlServer
echo DatabaseProvider=SqlServer > .env.temp

echo.
echo 7. Testing SQL Server Configuration...
echo Note: This will only work if SQL Server is installed and running
echo You can check the configuration by running:
echo   dotnet run --no-build

echo.
echo ========================================
echo Test Complete!
echo ========================================
echo.
echo To manually test database switching:
echo 1. Edit appsettings.json
echo 2. Change "DatabaseProvider" value
echo 3. Run: dotnet run --no-build
echo.
echo API Endpoints to test:
echo - GET /api/database/info (requires admin token)
echo - GET /api/database/test-connection (requires admin token)
echo.

del .env.temp >nul 2>&1
pause
