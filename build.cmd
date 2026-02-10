@echo off
REM Build script for FSMP project
REM Uses Visual Studio MSBuild to support COM references

set MSBUILD="C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe"
set SOLUTION="FSMP.UI\FSMP.UI.Console\FsmpConsole\FsmpConsole.slnx"

echo Building FSMP solution...
%MSBUILD% %SOLUTION% -t:Build -p:Configuration=Debug -p:Platform=ARM64 -v:minimal

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b 1
)

echo Build succeeded!
exit /b 0
