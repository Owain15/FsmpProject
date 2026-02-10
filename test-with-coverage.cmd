@echo off
REM Test with coverage script for FSMP project

echo Building solution...
call build.cmd
if %ERRORLEVEL% NEQ 0 exit /b 1

echo Running tests with coverage...
dotnet test FSMP.Tests\FSMP.Tests.csproj --no-build --collect:"XPlat Code Coverage" --results-directory:.\coverage -- RunConfiguration.TargetPlatform=ARM64

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
