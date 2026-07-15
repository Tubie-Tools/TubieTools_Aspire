#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Building TubieTools_Aspire Solutions...${NC}"

# Clean previous builds
echo -e "${YELLOW}Cleaning previous builds...${NC}"
rm -rf bin obj

# Restore dependencies
echo -e "${YELLOW}Restoring dependencies...${NC}"
dotnet restore

if [ $? -ne 0 ]; then
	echo -e "${RED}Failed to restore dependencies${NC}"
	exit 1
fi

# Build solution
echo -e "${YELLOW}Building solution...${NC}"
dotnet build -c Release

if [ $? -ne 0 ]; then
	echo -e "${RED}Failed to build solution${NC}"
	exit 1
fi

# Run tests
echo -e "${YELLOW}Running tests...${NC}"
dotnet test TubieTools_Aspire.Tests -c Release --filter "PublicAPI" --logger "console;verbosity=normal"

if [ $? -ne 0 ]; then
	echo -e "${RED}Tests failed${NC}"
	exit 1
fi

# Build Docker image
echo -e "${YELLOW}Building Docker image...${NC}"
docker build -t tubie-tools/aspire:latest .

if [ $? -ne 0 ]; then
	echo -e "${RED}Failed to build Docker image${NC}"
	exit 1
fi

echo -e "${GREEN}Build completed successfully!${NC}"
echo -e "${GREEN}Docker image: tubie-tools/aspire:latest${NC}"
echo -e "${YELLOW}To run with docker-compose: docker-compose up -d${NC}"
