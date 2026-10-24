@echo off
title Launch Unity 6 Editor
cd /d "%~dp0"
echo Opening project in Unity 6 Editor...
start "" "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe" -projectPath "%~dp0." -openfile "Assets\_Project\Scenes\LaboratoryLevel.unity"
exit
