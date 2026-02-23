@echo off
REM Build script for FSMP project
REM Uses Visual Studio MSBuild to support COM references
REM Usage: build.cmd [ARM64|x64]  (default: x64)

set MSBUILD="C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe"
set SOLUTION="FSMP.UI\FSMP.UI.Console\FsmpConsole\FsmpConsole.slnx"

if "%1"=="" (
    set PLATFORM=x64
) else (
    set PLATFORM=%1
)

echo Building FSMP solution [%PLATFORM%]...
%MSBUILD% %SOLUTION% -t:Build -p:Configuration=Debug -p:Platform=%PLATFORM% -v:minimal

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b 1
)

echo Build succeeded!
exit /b 0
