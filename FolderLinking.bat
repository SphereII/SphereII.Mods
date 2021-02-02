@echo off

REM
REM Merges multiple folders so that the DMT Tool can build multiple directories

REM this folder is where the main DMT Mods folder is.
set TARGET_FOLDER=D:\Github\SphereII.ModsA19\

REM this is an array of different folders to merge
set TARGET="D:\Github\A19DMTMods\*-*"
for /d %%i in (%TARGET%) do  call :$CreateLink "%%i"	

SET TARGET="D:\Github\A19Mods\*-*"
for /d %%i in (%TARGET%) do  call :$CreateLink "%%i"	
pause
exit /B

::**************************************************
:$CreateLink
::**************************************************
echo Creating link for %~nx1
set NEW_FOLDER=%TARGET_FOLDER%\%~nx1
RD %NEW_FOLDER%
MKLINK /D  %NEW_FOLDER% %1
exit /B