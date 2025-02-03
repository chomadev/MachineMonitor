#!/bin/bash

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}Iniciando build do SystemChecker...${NC}"

# Restaurar pacotes
echo -e "${YELLOW}Restaurando pacotes...${NC}"
dotnet restore
if [ $? -ne 0 ]; then
    echo -e "${RED}Erro ao restaurar pacotes${NC}"
    exit 1
fi

# Build
echo -e "${YELLOW}Executando build...${NC}"
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo -e "${RED}Erro no build${NC}"
    exit 1
fi

# Testes
echo -e "${YELLOW}Executando testes...${NC}"
dotnet test --no-build --configuration Release
if [ $? -ne 0 ]; then
    echo -e "${RED}Erro nos testes${NC}"
    exit 1
fi

# Executar a aplicação
echo -e "${GREEN}Iniciando a aplicação...${NC}"
dotnet run --project SystemChecker.WPF --no-build --configuration Release & 