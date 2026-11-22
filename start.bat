@echo off
title P2P Tool Library - ASP.NET Core
echo ============================================
echo   P2P Tool Library - starting server...
echo ============================================
echo.

REM Check SQL Server is reachable (quick TCP check on 1433)
powershell -Command "try { $c = New-Object Net.Sockets.TcpClient; $c.Connect('localhost',1433); $c.Close(); exit 0 } catch { exit 1 }" >nul 2>&1
if errorlevel 1 (
    echo [WARN] SQL Server not reachable on localhost:1433
    echo        Make sure the SQL Server service is running.
    echo        The app may fail to load data until it is.
    echo.
)

echo Starting on http://localhost:5080 ...
echo.
dotnet run --urls http://localhost:5080
pause
