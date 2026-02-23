@echo off
REM Test script for FSMP project
REM Builds with MSBuild, then runs tests pointing at the platform-specific output
REM Usage: test.cmd [ARM64|x64]  (default: x64)

if "%1"=="" (
    set PLATFORM=x64
) else (
    set PLATFORM=%1
)

echo Building solution [%PLATFORM%]...
call build.cmd %PLATFORM%
if %ERRORLEVEL% NEQ 0 exit /b 1

echo Running tests [%PLATFORM%]...
dotnet test FSMP.Tests\bin\%PLATFORM%\Debug\net10.0\FSMP.Tests.dll --verbosity normal

if %ERRORLEVEL% NEQ 0 (
    echo Tests failed!
    exit /b 1
)

echo All tests passed!
exit /b 0
