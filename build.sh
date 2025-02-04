#!/bin/bash

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}Starting SystemChecker build...${NC}"

# Restaurar pacotes
echo -e "${YELLOW}Restoring packages...${NC}"
dotnet restore
if [ $? -ne 0 ]; then
    echo -e "${RED}Error restoring packages${NC}"
    exit 1
fi

# Build
echo -e "${YELLOW}Running build...${NC}"
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo -e "${RED}Build error${NC}"
    exit 1
fi

# Testes
echo -e "${YELLOW}Running tests...${NC}"
dotnet test --no-build --configuration Release
if [ $? -ne 0 ]; then
    echo -e "${RED}Test error${NC}"
    exit 1
fi

# Executar a aplicação
echo -e "${GREEN}Starting the application...${NC}"
dotnet run --project SystemChecker.WPF --no-build --configuration Release & 