@echo off
setlocal

set PORT=5000

for /f "tokens=5" %%a in ('netstat -aon ^| findstr :%PORT%') do (
    set /a "PID=%%a"
    goto :killProcess
)

:killProcess
if defined PID (
    echo Stopping process on port %PORT% with PID %PID%...
    taskkill /F /PID %PID%
    echo Process stopped.
) else (
    echo No process found running on port %PORT%.
)

timeout /t 4 >nul

endlocal
