@echo off
REM Run this script from the solution directory

REM Set the relative path to ILMerge.exe (adjust version and path if necessary)
set ILMERGE_PATH=.\packages\ILMerge.3.0.41\tools\net452\ILMerge.exe

REM Set the main assembly (dark_cheat.dll) location in the project bin\Debug folder
set MAIN_ASSEMBLY=.\d.a.r.k. cheat\bin\Debug\dark_cheat.dll

REM Set the directory where dependency DLLs are stored (in the Resources folder)
set LIB_DIR=.\d.a.r.k. cheat\Resources

REM Set the output path for the merged DLL (will be placed in bin\Debug)
set OUTPUT=.\d.a.r.k. cheat\bin\Debug\dark_cheat.dll

REM List of dependency DLLs (assumed to be located in the LIB_DIR)
set DEPENDENCIES=0Harmony.dll Mono.Cecil.dll Mono.Cecil.Mdb.dll Mono.Cecil.Pdb.dll Mono.Cecil.Rocks.dll MonoMod.RuntimeDetour.dll MonoMod.Utils.dll

echo Merging %MAIN_ASSEMBLY% with dependencies from %LIB_DIR%...
%ILMERGE_PATH% /log /lib:"%LIB_DIR%" /out:"%OUTPUT%" "%MAIN_ASSEMBLY%" %DEPENDENCIES%

if %errorlevel% neq 0 (
    powershell -Command "Write-Host 'ILMerge failed with error %errorlevel%' -ForegroundColor Red"
    pause
    exit /b %errorlevel%
)

powershell -Command "Write-Host 'Merge complete: %OUTPUT%' -ForegroundColor Green"
pause
