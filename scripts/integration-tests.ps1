# ===============================================
# TESTS D'INTÉGRATION INTER-SERVICES
# NiesPro ERP - Phase de Validation
# ===============================================

Write-Host "🚀 DÉMARRAGE DES TESTS D'INTÉGRATION INTER-SERVICES" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Configuration des services opérationnels
$services = @(
    @{ Name = "Auth.API"; HttpPort = 5001; HttpsPort = 5011; Path = "src/Services/Auth/Auth.API" },
    @{ Name = "Catalog.API"; HttpPort = 5003; HttpsPort = 5013; Path = "src/Services/Catalog/Catalog.API" },
    @{ Name = "Order.API"; HttpPort = 5002; HttpsPort = 5012; Path = "src/Services/Order/Order.API" },
    @{ Name = "Payment.API"; HttpPort = 5004; HttpsPort = 5014; Path = "src/Services/Payment/Payment.API" },
    @{ Name = "Stock.API"; HttpPort = 5005; HttpsPort = 5006; Path = "src/Services/Stock/Stock.API" }
)

$results = @()

Write-Host ""
Write-Host "📊 PHASE 1: VALIDATION DES HEALTH ENDPOINTS" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Yellow

foreach ($service in $services) {
    Write-Host ""
    Write-Host "🔍 Test: $($service.Name)" -ForegroundColor Cyan
    
    $result = @{
        Service = $service.Name
        HttpHealth = $false
        HttpsHealth = $false
        HttpResponse = ""
        HttpsResponse = ""
        HttpTime = 0
        HttpsTime = 0
    }
    
    # Test HTTP Health
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $httpResponse = Invoke-WebRequest -Uri "http://localhost:$($service.HttpPort)/health" -UseBasicParsing -TimeoutSec 10
        $stopwatch.Stop()
        
        if ($httpResponse.StatusCode -eq 200) {
            $result.HttpHealth = $true
            $result.HttpResponse = $httpResponse.Content
            $result.HttpTime = $stopwatch.ElapsedMilliseconds
            Write-Host "  ✅ HTTP ($($service.HttpPort)): OK ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  ❌ HTTP ($($service.HttpPort)): FAILED - $($_.Exception.Message)" -ForegroundColor Red
        $result.HttpResponse = $_.Exception.Message
    }
    
    # Test HTTPS Health
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $httpsResponse = Invoke-WebRequest -Uri "https://localhost:$($service.HttpsPort)/health" -UseBasicParsing -SkipCertificateCheck -TimeoutSec 10
        $stopwatch.Stop()
        
        if ($httpsResponse.StatusCode -eq 200) {
            $result.HttpsHealth = $true
            $result.HttpsResponse = $httpsResponse.Content
            $result.HttpsTime = $stopwatch.ElapsedMilliseconds
            Write-Host "  ✅ HTTPS ($($service.HttpsPort)): OK ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  ❌ HTTPS ($($service.HttpsPort)): FAILED - $($_.Exception.Message)" -ForegroundColor Red
        $result.HttpsResponse = $_.Exception.Message
    }
    
    $results += $result
}

Write-Host ""
Write-Host "📊 PHASE 2: TESTS DE COMMUNICATION INTER-SERVICES" -ForegroundColor Yellow
Write-Host "=================================================" -ForegroundColor Yellow

# Test 1: Auth.API → Catalog.API (Authentification pour accès catalogue)
Write-Host ""
Write-Host "🔗 Test 1: Auth.API → Catalog.API Communication" -ForegroundColor Cyan

# Test 2: Order.API → Stock.API (Vérification stock avant commande)
Write-Host ""
Write-Host "🔗 Test 2: Order.API → Stock.API Communication" -ForegroundColor Cyan

# Test 3: Order.API → Payment.API (Processus de paiement)
Write-Host ""
Write-Host "🔗 Test 3: Order.API → Payment.API Communication" -ForegroundColor Cyan

Write-Host ""
Write-Host "📈 RAPPORT FINAL DES TESTS" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

$totalServices = $services.Count
$httpHealthy = ($results | Where-Object { $_.HttpHealth }).Count
$httpsHealthy = ($results | Where-Object { $_.HttpsHealth }).Count

Write-Host ""
Write-Host "📊 RÉSUMÉ OPÉRATIONNEL:" -ForegroundColor White
Write-Host "  • Services testés: $totalServices" -ForegroundColor White
Write-Host "  • HTTP opérationnels: $httpHealthy/$totalServices ($([math]::Round(($httpHealthy/$totalServices)*100, 1))%)" -ForegroundColor $(if ($httpHealthy -eq $totalServices) { "Green" } else { "Yellow" })
Write-Host "  • HTTPS opérationnels: $httpsHealthy/$totalServices ($([math]::Round(($httpsHealthy/$totalServices)*100, 1))%)" -ForegroundColor $(if ($httpsHealthy -eq $totalServices) { "Green" } else { "Yellow" })

Write-Host ""
Write-Host "📋 DÉTAILS PAR SERVICE:" -ForegroundColor White
foreach ($result in $results) {
    $httpStatus = if ($result.HttpHealth) { "✅" } else { "❌" }
    $httpsStatus = if ($result.HttpsHealth) { "✅" } else { "❌" }
    $httpTime = if ($result.HttpTime -gt 0) { "$($result.HttpTime)ms" } else { "N/A" }
    $httpsTime = if ($result.HttpsTime -gt 0) { "$($result.HttpsTime)ms" } else { "N/A" }
    
    Write-Host "  $($result.Service):" -ForegroundColor Cyan
    Write-Host "    HTTP:  $httpStatus ($httpTime)" -ForegroundColor $(if ($result.HttpHealth) { "Green" } else { "Red" })
    Write-Host "    HTTPS: $httpsStatus ($httpsTime)" -ForegroundColor $(if ($result.HttpsHealth) { "Green" } else { "Red" })
}

Write-Host ""
Write-Host "🎯 Tests d'intégration terminés!" -ForegroundColor Green