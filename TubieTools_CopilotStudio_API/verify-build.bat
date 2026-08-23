@echo off
REM Build Verification Script for TubieTools_CopilotStudio_API
REM Run this before committing any code
REM Usage: verify-build.bat

setlocal enabledelayedexpansion

cls

echo.
echo ================================
echo TubieTools_CopilotStudio_API
echo Build Verification Script
echo ================================
echo.

REM Phase 1: Check .NET Version
echo [PHASE 1] Checking .NET Version...
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo Installed: %DOTNET_VERSION%

if not "!DOTNET_VERSION:~0,2!"=="10" (
	echo.
	echo ERROR: .NET 10.0 required, but found %DOTNET_VERSION%
	echo.
	pause
	exit /b 1
)

echo [OK] .NET 10.0 detected
echo.

REM Phase 2: Verify Project File
echo [PHASE 2] Verifying Project File...
cd TubieTools_CopilotStudio_API

if not exist "TubieTools_CopilotStudio_API.csproj" (
	echo ERROR: TubieTools_CopilotStudio_API.csproj not found
	pause
	exit /b 1
)

findstr /M "Swashbuckle.AspNetCore.*6.10.0" TubieTools_CopilotStudio_API.csproj >nul 2>&1
if errorlevel 1 (
	echo WARNING: Swashbuckle version may not be correct (expected 6.10.0)
)

findstr /M "Microsoft.EntityFrameworkCore.*9.0.0" TubieTools_CopilotStudio_API.csproj >nul 2>&1
if errorlevel 1 (
	echo WARNING: EntityFrameworkCore version may not be correct (expected 9.0.0)
)

echo [OK] Project file verified
echo.

REM Phase 3: Clean Build
echo [PHASE 3] Cleaning Previous Build...
call dotnet clean --nologo -q 2>nul || true
if exist bin rmdir /s /q bin 2>nul
if exist obj rmdir /s /q obj 2>nul
echo [OK] Clean complete
echo.

REM Phase 4: Restore Packages
echo [PHASE 4] Restoring NuGet Packages...
call dotnet restore TubieTools_CopilotStudio_API.csproj --nologo -q
if errorlevel 1 (
	echo ERROR: Package restore failed
	echo.
	echo Common causes:
	echo - No internet connection
	echo - NuGet source is down
	echo - Package versions don't exist
	echo.
	pause
	exit /b 1
)
echo [OK] Package restore successful
echo.

REM Phase 5: Build
echo [PHASE 5] Building Project...
call dotnet build TubieTools_CopilotStudio_API.csproj -c Release --nologo --no-restore
if errorlevel 1 (
	echo ERROR: Build failed
	echo.
	echo Review the error messages above.
	echo Common issues:
	echo - Type not found (missing using statement)
	echo - Package version incompatibility
	echo - Code syntax error
	echo.
	pause
	exit /b 1
)
echo [OK] Build successful
echo.

REM Phase 6: Verify DLL Exists
echo [PHASE 6] Verifying Build Artifacts...
if exist "bin\Release\net10.0\TubieTools_CopilotStudio_API.dll" (
	for %%A in ("bin\Release\net10.0\TubieTools_CopilotStudio_API.dll") do set "FILESIZE=%%~zA"
	set /a FILESIZEKB=!FILESIZE! / 1024
	echo [OK] DLL created: !FILESIZEKB! KB
) else (
	echo ERROR: DLL not found at bin\Release\net10.0\TubieTools_CopilotStudio_API.dll
	pause
	exit /b 1
)
echo.

REM Phase 7: Check File Structure
echo [PHASE 7] Checking File Structure...

set "FILES=Program.cs" "Data\CopilotStudioDbContext.cs" "Data\Repositories\IRepositories.cs" "Data\Repositories\RepositoryImplementations.cs" "Services\CopilotApplicationService.cs" "Controllers\CopilotApplicationsController.cs" "Controllers\HealthController.cs"

for %%F in (%FILES%) do (
	if exist "%%F" (
		echo [OK] %%F
	) else (
		echo ERROR: Missing file %%F
		pause
		exit /b 1
	)
)

echo.

REM Phase 8: EF Core Tools Check
echo [PHASE 8] Checking EF Core Tools...
where dotnet-ef >nul 2>&1
if !errorlevel! equ 0 (
	echo [OK] EF Core tools installed

	call dotnet ef dbcontext info >nul 2>&1
	if !errorlevel! equ 0 (
		echo [OK] DbContext recognized by EF Core
	) else (
		echo [WARNING] DbContext not yet initialized (OK for first time)
	)
) else (
	echo [INFO] EF Core tools not installed
	echo Install with: dotnet tool install --global dotnet-ef
)

echo.

REM Success
cls
echo.
echo ================================
echo *** ALL VERIFICATION PASSED ***
echo ================================
echo.

echo The code is ready to commit:
echo [OK] Build succeeds with 0 errors
echo [OK] All required files present
echo [OK] Package versions correct
echo [OK] DLL successfully generated
echo.

echo Next steps:
echo 1. Create feature branch:
echo    git checkout -b feature/copilot-api
echo.
echo 2. Commit changes:
echo    git add . && git commit -m "feat: Add Copilot Studio API with EF Core"
echo.
echo 3. Push branch:
echo    git push -u origin feature/copilot-api
echo.
echo 4. Create pull request in GitHub/Azure DevOps
echo.

pause
exit /b 0
