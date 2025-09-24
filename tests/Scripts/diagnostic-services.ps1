# ===============================================
# Script de Diagnostic des Services - NiesPro
# ===============================================
# Description: Diagnostic complet des microservices
# Auteur: NiesPro Development Team
# Date: 23/09/2025
# ===============================================

param(
    [switch]$CheckPorts,
    [switch]$CheckHealth,
    [switch]$CheckDatabases,
    [switch]$All
)

# Configuration des services
$services = @{
    "Auth" = @{
        "Path" = "src\Services\Auth\Auth.API"
        "Port" = 5001
        "Database" = "NiesPro_Auth"
    }
    "Payment" = @{
        "Path" = "src\Services\Payment\Payment.API"
        "Port" = 5002
        "Database" = "NiesPro_Payment"
    }
    "Order" = @{
        "Path" = "src\Services\Order\Order.API"
        "Port" = 5003
        "Database" = "NiesPro_Order"
    }
    "Catalog" = @{
        "Path" = "src\Services\Catalog\Catalog.API"
        "Port" = 5004
        "Database" = "NiesPro_Catalog"
    }
    "Customer" = @{
        "Path" = "src\Services\Customer.API"
        "Port" = 5009
        "Database" = "NiesPro_Customer"
    }
}

function Write-Header {
    Clear-Host
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "DIAGNOSTIC DES SERVICES - NIESPRO" -ForegroundColor Cyan
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "Date: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""
}

function Test-ServicePort($port) {
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

function Test-ServiceHealth($serviceName, $port) {
    try {
        $url = "http://localhost:$port/health"
        $response = Invoke-WebRequest -Uri $url -Method GET -TimeoutSec 5 -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Host "  HEALTH: OK" -ForegroundColor Green
            return $true
        } else {
            Write-Host "  HEALTH: Status $($response.StatusCode)" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "  HEALTH: NON ACCESSIBLE" -ForegroundColor Red
        return $false
    }
}

function Test-DatabaseConnection($databaseName) {
    try {
        # Test simple de connexion MySQL
        $connectionString = "Server=localhost;Port=3306;Database=$databaseName;Uid=root;Pwd=;"
        $connection = New-Object MySql.Data.MySqlClient.MySqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        Write-Host "  DATABASE: OK ($databaseName)" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  DATABASE: ERREUR ($databaseName)" -ForegroundColor Red
        return $false
    }
}

function Show-ServiceStatus($serviceName, $config) {
    Write-Host "[$serviceName]" -ForegroundColor White -NoNewline
    Write-Host " Port:$($config.Port)" -ForegroundColor Gray
    
    # Test du port
    $portOpen = Test-ServicePort $config.Port
    if ($portOpen) {
        Write-Host "  PORT: OUVERT" -ForegroundColor Green
        
        # Test health si port ouvert
        if ($CheckHealth -or $All) {
            Test-ServiceHealth $serviceName $config.Port | Out-Null
        }
    } else {
        Write-Host "  PORT: FERME" -ForegroundColor Red
    }
    
    # Test base de données
    if ($CheckDatabases -or $All) {
        Test-DatabaseConnection $config.Database | Out-Null
    }
    
    Write-Host ""
}

# Début du diagnostic
Write-Header

if ($All -or $CheckPorts -or $CheckHealth -or $CheckDatabases) {
    Write-Host "Diagnostic des services en cours..." -ForegroundColor Yellow
    Write-Host ""
    
    foreach ($service in $services.GetEnumerator()) {
        Show-ServiceStatus $service.Key $service.Value
    }
    
    # Test MySQL global
    Write-Host "Test MySQL global:" -ForegroundColor Yellow
    $mysqlRunning = Test-ServicePort 3306
    if ($mysqlRunning) {
        Write-Host "  MySQL: ACTIF (Port 3306)" -ForegroundColor Green
    } else {
        Write-Host "  MySQL: INACTIF" -ForegroundColor Red
    }
    
} else {
    Write-Host "Usage: .\diagnostic-services.ps1 [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor White
    Write-Host "  -CheckPorts      : Vérifier les ports des services"
    Write-Host "  -CheckHealth     : Tester les endpoints de santé"
    Write-Host "  -CheckDatabases  : Vérifier les connexions aux bases"
    Write-Host "  -All             : Tous les tests"
    Write-Host ""
    Write-Host "Exemple: .\diagnostic-services.ps1 -All"
}

Write-Host ""
Write-Host "Diagnostic terminé: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray