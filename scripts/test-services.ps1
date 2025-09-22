# Tests d'integration inter-services NiesPro ERP
Write-Host "Demarrage des tests d'integration inter-services" -ForegroundColor Green

# Configuration des services operationnels
$services = @(
    @{ Name = "Auth.API"; HttpPort = 5001; HttpsPort = 5011 },
    @{ Name = "Catalog.API"; HttpPort = 5003; HttpsPort = 5013 },
    @{ Name = "Order.API"; HttpPort = 5002; HttpsPort = 5012 },
    @{ Name = "Payment.API"; HttpPort = 5004; HttpsPort = 5014 },
    @{ Name = "Stock.API"; HttpPort = 5005; HttpsPort = 5006 }
)

$results = @()

Write-Host "PHASE 1: VALIDATION DES HEALTH ENDPOINTS" -ForegroundColor Yellow

foreach ($service in $services) {
    Write-Host "Test: $($service.Name)" -ForegroundColor Cyan
    
    $result = @{
        Service = $service.Name
        HttpHealth = $false
        HttpsHealth = $false
        HttpTime = 0
        HttpsTime = 0
    }
    
    # Test HTTP Health
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $httpResponse = Invoke-WebRequest -Uri "http://localhost:$($service.HttpPort)/health" -UseBasicParsing -TimeoutSec 5
        $stopwatch.Stop()
        
        if ($httpResponse.StatusCode -eq 200) {
            $result.HttpHealth = $true
            $result.HttpTime = $stopwatch.ElapsedMilliseconds
            Write-Host "  HTTP ($($service.HttpPort)): OK ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  HTTP ($($service.HttpPort)): FAILED" -ForegroundColor Red
    }
    
    # Test HTTPS Health
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $httpsResponse = Invoke-WebRequest -Uri "https://localhost:$($service.HttpsPort)/health" -UseBasicParsing -SkipCertificateCheck -TimeoutSec 5
        $stopwatch.Stop()
        
        if ($httpsResponse.StatusCode -eq 200) {
            $result.HttpsHealth = $true
            $result.HttpsTime = $stopwatch.ElapsedMilliseconds
            Write-Host "  HTTPS ($($service.HttpsPort)): OK ($($stopwatch.ElapsedMilliseconds)ms)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  HTTPS ($($service.HttpsPort)): FAILED" -ForegroundColor Red
    }
    
    $results += $result
}

Write-Host "RAPPORT FINAL DES TESTS" -ForegroundColor Green

$totalServices = $services.Count
$httpHealthy = ($results | Where-Object { $_.HttpHealth }).Count
$httpsHealthy = ($results | Where-Object { $_.HttpsHealth }).Count

Write-Host "Services testes: $totalServices" -ForegroundColor White
Write-Host "HTTP operationnels: $httpHealthy/$totalServices" -ForegroundColor White
Write-Host "HTTPS operationnels: $httpsHealthy/$totalServices" -ForegroundColor White

Write-Host "DETAILS PAR SERVICE:" -ForegroundColor White
foreach ($result in $results) {
    $httpStatus = if ($result.HttpHealth) { "OK" } else { "FAILED" }
    $httpsStatus = if ($result.HttpsHealth) { "OK" } else { "FAILED" }
    
    Write-Host "$($result.Service): HTTP=$httpStatus, HTTPS=$httpsStatus" -ForegroundColor Cyan
}

Write-Host "Tests d'integration termines!" -ForegroundColor Green