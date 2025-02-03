# Script de Build para SystemChecker

Write-Host "Iniciando build do SystemChecker..." -ForegroundColor Green

# Restaurar pacotes
Write-Host "Restaurando pacotes..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao restaurar pacotes" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build
Write-Host "Executando build..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro no build" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Testes
Write-Host "Executando testes..." -ForegroundColor Yellow
dotnet test --no-build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro nos testes" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Executar a aplicação
Write-Host "Iniciando a aplicação..." -ForegroundColor Green
Start-Process dotnet -ArgumentList "run --project SystemChecker.WPF --no-build --configuration Release"
exit