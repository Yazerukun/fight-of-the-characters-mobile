@echo off
title Fight of the Characters 3D Launcher
cd /d "d:\fight of the characters apk"

echo ========================================================================
echo   FIGHT OF THE CHARACTERS 3D (WARCRAFT 3 CLASH)
echo ========================================================================
echo.
echo Starting local 3D game server on port 3000...
powershell -Command "if (-not (Get-NetTCPConnection -LocalPort 3000 -ErrorAction SilentlyContinue)) { Start-Process python -ArgumentList '-m http.server 3000 --bind 0.0.0.0' -WindowStyle Hidden }"

echo Opening browser at http://localhost:3000/ ...
start http://localhost:3000/

echo Game started successfully!
echo (You can also open on your mobile phone via Wi-Fi: http://192.168.27.32:3000/)
echo.
pause
