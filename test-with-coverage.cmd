@echo off
REM Test with coverage script for FSMP project
REM Usage: test-with-coverage.cmd [ARM64|x64]  (default: x64)

if "%1"=="" (
    set PLATFORM=x64
) else (
    set PLATFORM=%1
)

echo Building solution [%PLATFORM%]...
call build.cmd %PLATFORM%
if %ERRORLEVEL% NEQ 0 exit /b 1

echo Running tests with coverage [%PLATFORM%]...
dotnet test FSMP.Tests\bin\%PLATFORM%\Debug\net10.0\FSMP.Tests.dll --collect:"XPlat Code Coverage" --results-directory:.\coverage

if %ERRORLEVEL% NEQ 0 (
    echo Tests failed!
    exit /b 1
)

echo Generating coverage report...
dotnet tool install -g dotnet-reportgenerator-globaltool 2>NUL:
reportgenerator -reports:".\coverage\**\coverage.cobertura.xml" -targetdir:".\coverage\report" -reporttypes:Html

echo.
echo Coverage report generated at: coverage\report\index.html
echo.
exit /b 0
