# Script de Test Simple - NiesPro

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Tests NIESPRO - Demarrage" -ForegroundColor Cyan  
Write-Host "===============================================" -ForegroundColor Cyan

$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

# Test compilation globale
Write-Host ""
Write-Host "Test 1: Compilation globale" -ForegroundColor Yellow
Push-Location $projectRoot
dotnet build --no-restore
$globalBuildSuccess = $LASTEXITCODE -eq 0
Pop-Location

if ($globalBuildSuccess) {
    Write-Host "Compilation globale: SUCCES" -ForegroundColor Green
} else {
    Write-Host "Compilation globale: ECHEC" -ForegroundColor Red
}

# Test services individuels
Write-Host ""
Write-Host "Test 2: Services individuels" -ForegroundColor Yellow

$services = @("Auth", "Payment", "Order", "Catalog")
$serviceResults = @{}

foreach ($service in $services) {
    $servicePath = Join-Path $projectRoot "src\Services\$service\$service.API"
    
    if (Test-Path $servicePath) {
        Write-Host "  Test $service.API..." -ForegroundColor White
        
        Push-Location $servicePath
        dotnet build --no-restore --verbosity quiet
        $buildSuccess = $LASTEXITCODE -eq 0
        Pop-Location
        
        if ($buildSuccess) {
            Write-Host "  $service : SUCCES" -ForegroundColor Green
            $serviceResults[$service] = $true
        } else {
            Write-Host "  $service : ECHEC" -ForegroundColor Red
            $serviceResults[$service] = $false
        }
    } else {
        Write-Host "  $service : NON TROUVE" -ForegroundColor Yellow
        $serviceResults[$service] = $null
    }
}

# Resume
Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "RESUME DES TESTS" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

$successCount = ($serviceResults.Values | Where-Object { $_ -eq $true }).Count
$totalCount = $serviceResults.Count

Write-Host "Compilation globale: $(if($globalBuildSuccess){'SUCCES'}else{'ECHEC'})"
Write-Host "Services reussis: $successCount/$totalCount"

if ($globalBuildSuccess -and $successCount -eq $totalCount) {
    Write-Host ""
    Write-Host "TOUS LES TESTS REUSSIS!" -ForegroundColor Green
    Write-Host "Projet NiesPro pret pour les tests fonctionnels" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "PROBLEMES DETECTES" -ForegroundColor Yellow
    Write-Host "Verifiez les erreurs ci-dessus" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Tests termines: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan