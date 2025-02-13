# Build script for SystemChecker

Write-Host "Starting SystemChecker build..." -ForegroundColor Green

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error restoring packages" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build
Write-Host "Executing build..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error on build" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Tests
Write-Host "Executing tests..." -ForegroundColor Yellow
dotnet test --no-build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error on tests" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Start the application
Write-Host "Starting the application..." -ForegroundColor Green
Start-Process dotnet -ArgumentList "run --project SystemChecker.WPF --no-build --configuration Release"
exit