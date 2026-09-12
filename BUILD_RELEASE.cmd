@echo off
setlocal
cd /d "%~dp0"

title Build Valheim Shared World Manager 2.0

echo ==============================================
echo  Valheim Shared World Manager 2.0 - Build
echo ==============================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: .NET 8 SDK was not found.
    echo.
    echo Install the .NET 8 SDK from Microsoft, then run this file again.
    echo The other player does NOT need the SDK after the EXE has been built.
    echo.
    pause
    exit /b 1
)

echo Cleaning previous publish...
if exist "%~dp0publish" rmdir /s /q "%~dp0publish"

echo Building self-contained Windows x64 EXE...
echo.

dotnet publish "ValheimSharedWorldManager\ValheimSharedWorldManager.csproj" ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=None ^
  -p:DebugSymbols=false ^
  -o "%~dp0publish"

if errorlevel 1 (
    echo.
    echo ==============================================
    echo  BUILD FAILED
    echo ==============================================
    echo.
    pause
    exit /b 1
)

echo.
echo ==============================================
echo  BUILD COMPLETE
echo ==============================================
echo.
echo Finished app:
echo %~dp0publish\ValheimSharedWorldManager.exe
echo.
echo You can copy that EXE to the other player's PC.
echo.
pause
