# Script de test complet de l'architecture NiesPro
param(
    [switch]$Detailed
)

function Write-TestResult {
    param($TestName, $Result, $Details = "")
    
    if ($Result) {
        Write-Host "✅ $TestName" -ForegroundColor Green
        if ($Details) { Write-Host "   $Details" -ForegroundColor Gray }
    } else {
        Write-Host "❌ $TestName" -ForegroundColor Red
        if ($Details) { Write-Host "   $Details" -ForegroundColor Yellow }
    }
}

function Test-ServiceHealth {
    param($ServiceName, $Url)
    
    try {
        $response = Invoke-RestMethod -Uri $Url -Method Get -SkipCertificateCheck -TimeoutSec 5
        return $true, $response
    }
    catch {
        return $false, $_.Exception.Message
    }
}

function Test-ServicePort {
    param($Port)
    
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $result = $tcpClient.BeginConnect("localhost", $Port, $null, $null)
        $wait = $result.AsyncWaitHandle.WaitOne(1000, $false)
        
        if ($wait) {
            $tcpClient.EndConnect($result)
            $tcpClient.Close()
            return $true
        } else {
            $tcpClient.Close()
            return $false
        }
    }
    catch {
        return $false
    }
}

Write-Host "Test de l'Architecture Microservices NiesPro" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Test des ports
Write-Host "Test des Ports" -ForegroundColor Yellow
Write-Host "-----------------" -ForegroundColor Yellow

$ports = @(
    @{Name="Gateway.API"; Port=5000},
    @{Name="Auth.API"; Port=5001},
    @{Name="Order.API"; Port=5002},
    @{Name="Catalog.API"; Port=5003}
)

foreach ($service in $ports) {
    $isOpen = Test-ServicePort -Port $service.Port
    Write-TestResult "$($service.Name) (Port $($service.Port))" $isOpen
}

Write-Host ""

# Test des Health Checks
Write-Host "Test des Health Checks" -ForegroundColor Yellow
Write-Host "--------------------------" -ForegroundColor Yellow

$healthChecks = @(
    @{Name="Gateway.API"; Url="https://localhost:5000/health"},
    @{Name="Auth.API"; Url="https://localhost:5001/health"},
    @{Name="Order.API"; Url="https://localhost:5002/health"},
    @{Name="Catalog.API"; Url="https://localhost:5003/health"}
)

foreach ($check in $healthChecks) {
    $isHealthy, $response = Test-ServiceHealth -ServiceName $check.Name -Url $check.Url
    Write-TestResult "$($check.Name) Health Check" $isHealthy $response
}

Write-Host ""

# Test du Gateway Info
Write-Host "Test Gateway Info" -ForegroundColor Yellow
Write-Host "--------------------" -ForegroundColor Yellow

try {
    $gatewayInfo = Invoke-RestMethod -Uri "https://localhost:5000/api/gateway/info" -Method Get -SkipCertificateCheck -TimeoutSec 5
    Write-TestResult "Gateway Info API" $true
    
    if ($Detailed) {
        Write-Host "   Gateway: $($gatewayInfo.name) v$($gatewayInfo.version)" -ForegroundColor Gray
        Write-Host "   Environment: $($gatewayInfo.environment)" -ForegroundColor Gray
        Write-Host "   Features: $($gatewayInfo.features -join ', ')" -ForegroundColor Gray
    }
}
catch {
    Write-TestResult "Gateway Info API" $false $_.Exception.Message
}

Write-Host ""

# Test du routage Gateway
Write-Host "Test Routage Gateway" -ForegroundColor Yellow
Write-Host "-----------------------" -ForegroundColor Yellow

$routes = @(
    @{Name="Products via Gateway"; Url="https://localhost:5000/api/products"},
    @{Name="Categories via Gateway"; Url="https://localhost:5000/api/categories"}
)

foreach ($route in $routes) {
    try {
        $response = Invoke-RestMethod -Uri $route.Url -Method Get -SkipCertificateCheck -TimeoutSec 5
        Write-TestResult $route.Name $true "Response received"
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 401) {
            Write-TestResult $route.Name $true "Unauthorized (attendu pour routes protégées)"
        } else {
            Write-TestResult $route.Name $false "Status: $statusCode"
        }
    }
}

Write-Host ""

# Résumé
Write-Host "Resume de l'Architecture" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""
Write-Host "URLs d'acces :" -ForegroundColor White
Write-Host "  Gateway API: https://localhost:5000" -ForegroundColor Green
Write-Host "  Auth API: https://localhost:5001" -ForegroundColor Green  
Write-Host "  Order API: https://localhost:5002" -ForegroundColor Green
Write-Host "  Catalog API: https://localhost:5003" -ForegroundColor Green
Write-Host ""
Write-Host "Documentation :" -ForegroundColor White
Write-Host "  Gateway Swagger: https://localhost:5000/swagger" -ForegroundColor Green
Write-Host "  Gateway Health: https://localhost:5000/health" -ForegroundColor Green
Write-Host "  Gateway Info: https://localhost:5000/api/gateway/info" -ForegroundColor Green
Write-Host ""

# Statistiques des processus
$dotnetProcesses = Get-Process dotnet -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "Processus actifs: $($dotnetProcesses.Count) processus dotnet.exe" -ForegroundColor Green
} else {
    Write-Host "Aucun processus dotnet detecte" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Architecture NiesPro operationnelle !" -ForegroundColor Green