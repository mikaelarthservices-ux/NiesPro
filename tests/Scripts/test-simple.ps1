# ===============================================
# Script de Test Simple - NiesPro
# ===============================================

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "🧪 TESTS RAPIDES - NIESPRO" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

$projectRoot = Split-Path $PSScriptRoot -Parent

# Test 1: Compilation globale
Write-Host ""
Write-Host "🔧 Test 1: Compilation globale" -ForegroundColor Yellow
Push-Location $projectRoot
dotnet build --no-restore
$globalBuildSuccess = $LASTEXITCODE -eq 0
Pop-Location

if ($globalBuildSuccess) {
    Write-Host "✅ Compilation globale: SUCCÈS" -ForegroundColor Green
} else {
    Write-Host "❌ Compilation globale: ÉCHEC" -ForegroundColor Red
}

# Test 2: Services individuels
Write-Host ""
Write-Host "🔧 Test 2: Services individuels" -ForegroundColor Yellow

$services = @("Auth", "Payment", "Order", "Catalog")
$serviceResults = @{}

foreach ($service in $services) {
    $servicePath = Join-Path $projectRoot "src\Services\$service\$service.API"
    
    if (Test-Path $servicePath) {
        Write-Host "  📦 Test $service.API..." -ForegroundColor White
        
        Push-Location $servicePath
        dotnet build --no-restore --verbosity quiet
        $buildSuccess = $LASTEXITCODE -eq 0
        Pop-Location
        
        if ($buildSuccess) {
            Write-Host "  ✅ $service : SUCCÈS" -ForegroundColor Green
            $serviceResults[$service] = $true
        } else {
            Write-Host "  ❌ $service : ÉCHEC" -ForegroundColor Red
            $serviceResults[$service] = $false
        }
    } else {
        Write-Host "  ⚠️  $service : NON TROUVÉ" -ForegroundColor Yellow
        $serviceResults[$service] = $null
    }
}

# Test 3: Validation des ports (si services existent)
Write-Host ""
Write-Host "🔧 Test 3: Validation des ports" -ForegroundColor Yellow

$ports = @{
    "Auth" = 5001
    "Payment" = 5002
    "Order" = 5003
    "Catalog" = 5004
}

foreach ($service in $ports.Keys) {
    $port = $ports[$service]
    
    try {
        $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Any, $port)
        $listener.Start()
        $listener.Stop()
        Write-Host "  ✅ Port $port ($service) : LIBRE" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠️  Port $port ($service) : OCCUPÉ" -ForegroundColor Yellow
    }
}

# Résumé
Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "📊 RÉSUMÉ DES TESTS" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

$successCount = ($serviceResults.Values | Where-Object { $_ -eq $true }).Count
$totalCount = $serviceResults.Count

Write-Host "Compilation globale: $(if($globalBuildSuccess){'✅ SUCCÈS'}else{'❌ ÉCHEC'})"
Write-Host "Services réussis: $successCount/$totalCount"

if ($globalBuildSuccess -and $successCount -eq $totalCount) {
    Write-Host ""
    Write-Host "🎉 TOUS LES TESTS RÉUSSIS!" -ForegroundColor Green
    Write-Host "✅ Projet NiesPro prêt pour les tests fonctionnels" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "⚠️  PROBLÈMES DÉTECTÉS" -ForegroundColor Yellow
    Write-Host "❗ Vérifiez les erreurs ci-dessus" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Tests terminés: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan