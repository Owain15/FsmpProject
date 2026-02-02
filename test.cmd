@echo off
REM Test script for FSMP project
REM Builds with MSBuild, then runs tests with dotnet test --no-build

echo Building solution...
call build.cmd
if %ERRORLEVEL% NEQ 0 exit /b 1

echo Running tests...
dotnet test FSMP.Tests\FSMP.Tests.csproj --no-build --verbosity normal

if %ERRORLEVEL% NEQ 0 (
    echo Tests failed!
    exit /b 1
)

echo All tests passed!
exit /b 0
