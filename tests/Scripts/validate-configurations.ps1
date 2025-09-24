# =================================================================
# SCRIPT DE VALIDATION DES CONFIGURATIONS - NIESPRO
# =================================================================
# Ce script vérifie l'alignement entre les configurations des services
# et les attentes du Gateway API

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "VALIDATION DES CONFIGURATIONS - NIESPRO" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Heure: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Configuration attendue par le Gateway
$expectedPorts = @{
    "AuthAPI" = @{ "Http" = 5001; "Https" = 5011; "GatewayExpected" = "https://localhost:5011" }
    "CatalogAPI" = @{ "Http" = 5003; "Https" = 5013; "GatewayExpected" = "https://localhost:5013" }
    "OrderAPI" = @{ "Http" = 5002; "Https" = 5012; "GatewayExpected" = "https://localhost:5012" }
    "PaymentAPI" = @{ "Http" = 5004; "Https" = 5014; "GatewayExpected" = "https://localhost:5014" }
    "StockAPI" = @{ "Http" = 5005; "Https" = 5006; "GatewayExpected" = "https://localhost:5006" }
    "CustomerAPI" = @{ "Http" = 8001; "Https" = 8011; "GatewayExpected" = "http://localhost:8001" }
    "RestaurantAPI" = @{ "Http" = 7001; "Https" = 7011; "GatewayExpected" = "https://localhost:7011" }
}

# Fonction pour extraire la configuration d'un service
function Get-ServiceConfig {
    param($servicePath, $serviceName)
    
    $configPath = "$servicePath\appsettings.json"
    if (-not (Test-Path $configPath)) {
        return $null
    }
    
    try {
        $config = Get-Content $configPath -Raw | ConvertFrom-Json
        if ($config.Kestrel -and $config.Kestrel.Endpoints) {
            $httpUrl = $config.Kestrel.Endpoints.Http.Url
            $httpsUrl = $config.Kestrel.Endpoints.Https.Url
            
            return @{
                "HttpUrl" = $httpUrl
                "HttpsUrl" = $httpsUrl
                "HttpPort" = if ($httpUrl) { ([Uri]$httpUrl).Port } else { $null }
                "HttpsPort" = if ($httpsUrl) { ([Uri]$httpsUrl).Port } else { $null }
                "HttpHost" = if ($httpUrl) { ([Uri]$httpUrl).Host } else { $null }
                "HttpsHost" = if ($httpsUrl) { ([Uri]$httpsUrl).Host } else { $null }
            }
        }
    }
    catch {
        return $null
    }
    return $null
}

# Vérification des services
$services = @{
    "AuthAPI" = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Auth\Auth.API"
    "CatalogAPI" = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Catalog\Catalog.API"
    "OrderAPI" = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Order\Order.API"
    "PaymentAPI" = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Payment\Payment.API"
    "StockAPI" = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Stock\Stock.API"
    "CustomerAPI" = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Customer.API"
    "RestaurantAPI" = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Restaurant\Restaurant.API"
}

$allValid = $true

foreach ($serviceName in $services.Keys) {
    $servicePath = $services[$serviceName]
    $expected = $expectedPorts[$serviceName]
    $actual = Get-ServiceConfig -servicePath $servicePath -serviceName $serviceName
    
    Write-Host "OK $serviceName" -ForegroundColor Yellow
    
    if (-not $actual) {
        Write-Host "   ERREUR Configuration Kestrel manquante" -ForegroundColor Red
        $allValid = $false
        continue
    }
    
    # Vérification des ports
    $httpPortOk = $actual.HttpPort -eq $expected.Http
    $httpsPortOk = $actual.HttpsPort -eq $expected.Https
    
    # Vérification des hosts
    $hostOk = ($actual.HttpHost -eq "localhost") -and ($actual.HttpsHost -eq "localhost")
    
    if ($httpPortOk -and $httpsPortOk -and $hostOk) {
        Write-Host "   OK Configuration correcte" -ForegroundColor Green
        Write-Host "      HTTP: $($actual.HttpUrl)" -ForegroundColor Gray
        Write-Host "      HTTPS: $($actual.HttpsUrl)" -ForegroundColor Gray
        Write-Host "      Gateway attend: $($expected.GatewayExpected)" -ForegroundColor Gray
    } else {
        Write-Host "   ERREUR Configuration incorrecte" -ForegroundColor Red
        if (-not $httpPortOk) {
            Write-Host "      Port HTTP: Attendu $($expected.Http), Trouvé $($actual.HttpPort)" -ForegroundColor Red
        }
        if (-not $httpsPortOk) {
            Write-Host "      Port HTTPS: Attendu $($expected.Https), Trouvé $($actual.HttpsPort)" -ForegroundColor Red
        }
        if (-not $hostOk) {
            Write-Host "      Host: Doit être 'localhost', Trouvé HTTP:'$($actual.HttpHost)' HTTPS:'$($actual.HttpsHost)'" -ForegroundColor Red
        }
        Write-Host "      Configuration actuelle:" -ForegroundColor Yellow
        Write-Host "        HTTP: $($actual.HttpUrl)" -ForegroundColor Yellow
        Write-Host "        HTTPS: $($actual.HttpsUrl)" -ForegroundColor Yellow
        Write-Host "      Gateway attend: $($expected.GatewayExpected)" -ForegroundColor Yellow
        $allValid = $false
    }
    Write-Host ""
}

# Vérification de la configuration Gateway
Write-Host "Gateway.API" -ForegroundColor Yellow
$gatewayConfigPath = "C:\Users\HP\Documents\projets\NiesPro\src\Gateway\Gateway.API\appsettings.json"
if (Test-Path $gatewayConfigPath) {
    try {
        $gatewayConfig = Get-Content $gatewayConfigPath -Raw | ConvertFrom-Json
        Write-Host "   OK Configuration Gateway trouvee" -ForegroundColor Green
        
        # Vérification que les URLs du Gateway correspondent aux configurations des services
        $microservices = $gatewayConfig.Microservices
        foreach ($serviceName in $expectedPorts.Keys) {
            $gatewayServiceName = $serviceName -replace "API", "API"
            if ($microservices.$gatewayServiceName) {
                $gatewayExpected = $expectedPorts[$serviceName].GatewayExpected
                $gatewayActual = $microservices.$gatewayServiceName.BaseUrl
                
                if ($gatewayExpected -eq $gatewayActual) {
                    Write-Host "   OK $serviceName URLs alignees" -ForegroundColor Green
                } else {
                    Write-Host "   ERREUR $serviceName URLs non alignees" -ForegroundColor Red
                    Write-Host "      Gateway config: $gatewayActual" -ForegroundColor Red
                    Write-Host "      Service config: $gatewayExpected" -ForegroundColor Red
                    $allValid = $false
                }
            }
        }
    }
    catch {
        Write-Host "   ERREUR Erreur lors de la lecture de la configuration Gateway" -ForegroundColor Red
        $allValid = $false
    }
} else {
    Write-Host "   ERREUR Configuration Gateway non trouvee" -ForegroundColor Red
    $allValid = $false
}

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
if ($allValid) {
    Write-Host "OK TOUTES LES CONFIGURATIONS SONT VALIDES" -ForegroundColor Green
} else {
    Write-Host "ERREUR DES CORRECTIONS SONT NECESSAIRES" -ForegroundColor Red
}
Write-Host "===============================================" -ForegroundColor Cyan