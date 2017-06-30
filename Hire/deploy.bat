
@echo off

set H=R:\KSP_1.3.0_dev
set GAMEDIR=TRP-Hire

echo %H%

copy /Y "%1%2" "GameData\%GAMEDIR%"
copy /Y %GAMEDIR%.version GameData\%GAMEDIR%

mkdir "%H%\GameData\%GAMEDIR%"
xcopy /y /s GameData\TRP-Hire "%H%\GameData\%GAMEDIR%"

pause