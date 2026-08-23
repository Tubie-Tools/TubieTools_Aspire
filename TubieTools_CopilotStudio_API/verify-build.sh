#!/bin/bash

# Build Verification Script for TubieTools_CopilotStudio_API
# Run this before committing any code
# Usage: ./verify-build.sh

set -e  # Exit on any error

RESET='\033[0m'
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'

echo -e "${BLUE}================================${RESET}"
echo -e "${BLUE}TubieTools_CopilotStudio_API${RESET}"
echo -e "${BLUE}Build Verification Script${RESET}"
echo -e "${BLUE}================================${RESET}\n"

# Phase 1: Check .NET Version
echo -e "${YELLOW}[PHASE 1] Checking .NET Version...${RESET}"
DOTNET_VERSION=$(dotnet --version)
echo "Installed: $DOTNET_VERSION"

if [[ $DOTNET_VERSION == 10.* ]]; then
	echo -e "${GREEN}✓ .NET 10.0 detected${RESET}\n"
else
	echo -e "${RED}✗ ERROR: .NET 10.0 required, but found $DOTNET_VERSION${RESET}"
	exit 1
fi

# Phase 2: Verify Project File
echo -e "${YELLOW}[PHASE 2] Verifying Project File...${RESET}"
cd TubieTools_CopilotStudio_API

if [ ! -f "TubieTools_CopilotStudio_API.csproj" ]; then
	echo -e "${RED}✗ ERROR: TubieTools_CopilotStudio_API.csproj not found${RESET}"
	exit 1
fi

# Check package versions in csproj
if grep -q 'Swashbuckle.AspNetCore.*6.10.0' TubieTools_CopilotStudio_API.csproj; then
	echo -e "${GREEN}✓ Swashbuckle.AspNetCore 6.10.0 correct${RESET}"
else
	echo -e "${RED}✗ ERROR: Swashbuckle version incorrect${RESET}"
	exit 1
fi

if grep -q 'Microsoft.EntityFrameworkCore.*9.0.0' TubieTools_CopilotStudio_API.csproj; then
	echo -e "${GREEN}✓ EntityFrameworkCore 9.0.0 correct${RESET}"
else
	echo -e "${RED}✗ ERROR: EntityFrameworkCore version incorrect${RESET}"
	exit 1
fi

echo ""

# Phase 3: Clean Build
echo -e "${YELLOW}[PHASE 3] Cleaning Previous Build...${RESET}"
dotnet clean --nologo -q || true
rm -rf bin/ obj/
echo -e "${GREEN}✓ Clean complete${RESET}\n"

# Phase 4: Restore Packages
echo -e "${YELLOW}[PHASE 4] Restoring NuGet Packages...${RESET}"
if dotnet restore TubieTools_CopilotStudio_API.csproj --nologo -q; then
	echo -e "${GREEN}✓ Package restore successful${RESET}\n"
else
	echo -e "${RED}✗ ERROR: Package restore failed${RESET}"
	exit 1
fi

# Phase 5: Build
echo -e "${YELLOW}[PHASE 5] Building Project...${RESET}"
if dotnet build TubieTools_CopilotStudio_API.csproj -c Release --nologo --no-restore; then
	echo -e "${GREEN}✓ Build successful${RESET}\n"
else
	echo -e "${RED}✗ ERROR: Build failed${RESET}"
	exit 1
fi

# Phase 6: Verify DLL Exists
echo -e "${YELLOW}[PHASE 6] Verifying Build Artifacts...${RESET}"
if [ -f "bin/Release/net10.0/TubieTools_CopilotStudio_API.dll" ]; then
	DLL_SIZE=$(ls -lh bin/Release/net10.0/TubieTools_CopilotStudio_API.dll | awk '{print $5}')
	echo -e "${GREEN}✓ DLL created: $DLL_SIZE${RESET}\n"
else
	echo -e "${RED}✗ ERROR: DLL not found${RESET}"
	exit 1
fi

# Phase 7: Check File Structure
echo -e "${YELLOW}[PHASE 7] Checking File Structure...${RESET}"

REQUIRED_FILES=(
	"Program.cs"
	"Data/CopilotStudioDbContext.cs"
	"Data/Repositories/IRepositories.cs"
	"Data/Repositories/RepositoryImplementations.cs"
	"Services/CopilotApplicationService.cs"
	"Controllers/CopilotApplicationsController.cs"
	"Controllers/HealthController.cs"
)

for file in "${REQUIRED_FILES[@]}"; do
	if [ -f "$file" ]; then
		echo -e "${GREEN}✓ $file${RESET}"
	else
		echo -e "${RED}✗ Missing: $file${RESET}"
		exit 1
	fi
done

echo ""

# Phase 8: EF Core Tools Check (if installed)
echo -e "${YELLOW}[PHASE 8] Checking EF Core Tools...${RESET}"
if command -v dotnet-ef &> /dev/null; then
	echo -e "${GREEN}✓ EF Core tools installed${RESET}"

	# Try to get DbContext info
	if dotnet ef dbcontext info 2>/dev/null; then
		echo -e "${GREEN}✓ DbContext recognized by EF${RESET}"
	else
		echo -e "${YELLOW}⚠ DbContext not yet initialized (OK for first time)${RESET}"
	fi
else
	echo -e "${YELLOW}⚠ EF Core tools not installed (optional)${RESET}"
	echo -e "${YELLOW}  Install with: dotnet tool install --global dotnet-ef${RESET}"
fi

echo ""

# Summary
echo -e "${BLUE}================================${RESET}"
echo -e "${GREEN}✓✓✓ ALL VERIFICATION PHASES PASSED ✓✓✓${RESET}"
echo -e "${BLUE}================================${RESET}\n"

echo "The code is ready to commit:"
echo -e "${GREEN}✓ Build succeeds with 0 errors${RESET}"
echo -e "${GREEN}✓ All required files present${RESET}"
echo -e "${GREEN}✓ Package versions correct${RESET}"
echo -e "${GREEN}✓ DLL successfully generated${RESET}\n"

echo "Next steps:"
echo "1. Create feature branch: git checkout -b feature/copilot-api"
echo "2. Commit changes: git add . && git commit -m 'feat: Add Copilot Studio API with EF Core'"
echo "3. Push branch: git push -u origin feature/copilot-api"
echo "4. Create pull request"

exit 0
