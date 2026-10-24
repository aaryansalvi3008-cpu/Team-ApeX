@echo off
title Launch Escape the Abandoned Lab
cd /d "%~dp0"
echo Launching Escape the Abandoned Lab in Windowed Direct3D 11 mode...
start "" "Build\EscapeTheAbandonedLab.exe" -force-d3d11 -screen-width 1280 -screen-height 720 -screen-fullscreen 0
exit
