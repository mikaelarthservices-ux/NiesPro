# ===============================================
# Script de Test des Services - NiesPro
# ===============================================
# Description: Test du démarrage et santé des microservices
# Auteur: NiesPro Development Team
# Date: $(Get-Date -Format "dd/MM/yyyy")
# ===============================================

param(
    [switch]$StartServices,
    [switch]$StopServices,
    [switch]$CheckHealth,
    [int]$TimeoutSeconds = 30
)

# Configuration
$projectRoot = Split-Path $PSScriptRoot -Parent
$services = @{
    "Auth" = @{
        "Path" = "src\Services\Auth\Auth.API"
        "Port" = 5001
        "HealthEndpoint" = "/health"
    }
    "Payment" = @{
        "Path" = "src\Services\Payment\Payment.API"
        "Port" = 5002
        "HealthEndpoint" = "/health"
    }
    "Order" = @{
        "Path" = "src\Services\Order\Order.API"
        "Port" = 5003
        "HealthEndpoint" = "/health"
    }
    "Catalog" = @{
        "Path" = "src\Services\Catalog\Catalog.API"
        "Port" = 5004
        "HealthEndpoint" = "/health"
    }
}

# Couleurs pour l'affichage
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Success($message) { Write-ColorOutput Green $message }
function Write-Error($message) { Write-ColorOutput Red $message }
function Write-Warning($message) { Write-ColorOutput Yellow $message }
function Write-Info($message) { Write-ColorOutput Cyan $message }

function Show-Header {
    Clear-Host
    Write-Info "==============================================="
    Write-Info "🚀 TESTS DES SERVICES - NIESPRO"
    Write-Info "==============================================="
    Write-Info "Démarrage des tests: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"
    Write-Info "==============================================="
    Write-Output ""
}

# Fonction pour vérifier si un port est libre
function Test-Port($port) {
    try {
        $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Any, $port)
        $listener.Start()
        $listener.Stop()
        return $true
    } catch {
        return $false
    }
}

# Fonction pour tester la santé d'un service
function Test-ServiceHealth($serviceName, $port, $healthEndpoint) {
    try {
        $url = "http://localhost:$port$healthEndpoint"
        Write-Output "  🔍 Test santé: $url"
        
        $response = Invoke-WebRequest -Uri $url -Method GET -TimeoutSec 5 -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Success "  ✅ Service $serviceName: HEALTHY"
            return $true
        } else {
            Write-Warning "  ⚠️  Service $serviceName: Status $($response.StatusCode)"
            return $false
        }
    } catch {
        Write-Error "  ❌ Service $serviceName: Non accessible ($($_.Exception.Message))"
        return $false
    }
}

# Fonction pour démarrer un service
function Start-Service($serviceName, $serviceConfig) {
    Write-Info "🚀 Démarrage du service: $serviceName"
    
    $servicePath = Join-Path $projectRoot $serviceConfig.Path
    
    if (-not (Test-Path $servicePath)) {
        Write-Error "❌ Service $serviceName introuvable à: $servicePath"
        return $false
    }
    
    # Vérifier si le port est libre
    if (-not (Test-Port $serviceConfig.Port)) {
        Write-Warning "⚠️  Port $($serviceConfig.Port) déjà utilisé pour $serviceName"
        return $false
    }
    
    try {
        Push-Location $servicePath
        
        Write-Output "  📦 Compilation du service..."
        dotnet build --no-restore --verbosity quiet
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "  ❌ Échec de compilation pour $serviceName"
            Pop-Location
            return $false
        }
        
        Write-Output "  🔧 Démarrage sur le port $($serviceConfig.Port)..."
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run", "--no-build", "--urls", "http://localhost:$($serviceConfig.Port)" -PassThru -WindowStyle Hidden
        
        Pop-Location
        
        # Attendre que le service démarre
        $timeout = $TimeoutSeconds
        $started = $false
        
        while ($timeout -gt 0 -and -not $started) {
            Start-Sleep -Seconds 1
            $timeout--
            
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:$($serviceConfig.Port)/health" -Method GET -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
                if ($response.StatusCode -eq 200) {
                    $started = $true
                }
            } catch {
                # Service pas encore prêt
            }
            
            if ($timeout % 5 -eq 0) {
                Write-Output "  ⏳ Attente du démarrage... ($timeout secondes restantes)"
            }
        }
        
        if ($started) {
            Write-Success "  ✅ Service $serviceName démarré avec succès"
            return @{
                "Process" = $process
                "Port" = $serviceConfig.Port
                "Success" = $true
            }
        } else {
            Write-Error "  ❌ Timeout: Service $serviceName n'a pas démarré dans les temps"
            $process.Kill()
            return @{ "Success" = $false }
        }
        
    } catch {
        Write-Error "  ❌ Erreur lors du démarrage de $serviceName : $($_.Exception.Message)"
        Pop-Location
        return @{ "Success" = $false }
    }
}

# Fonction pour tester tous les services
function Test-AllServices {
    Write-Info "🔍 Test de santé de tous les services"
    Write-Output ""
    
    $results = @{}
    
    foreach ($serviceName in $services.Keys) {
        $config = $services[$serviceName]
        Write-Info "🔧 Test du service: $serviceName"
        
        $healthy = Test-ServiceHealth $serviceName $config.Port $config.HealthEndpoint
        $results[$serviceName] = $healthy
        
        Write-Output ""
    }
    
    return $results
}

# Fonction pour afficher le résumé
function Show-ServicesSummary($results) {
    Write-Info "==============================================="
    Write-Info "📊 RÉSUMÉ DES TESTS DE SERVICES"
    Write-Info "==============================================="
    
    $totalServices = $results.Keys.Count
    $healthyServices = ($results.Values | Where-Object { $_ -eq $true }).Count
    
    foreach ($serviceName in $results.Keys) {
        if ($results[$serviceName]) {
            Write-Success "✅ $serviceName : HEALTHY"
        } else {
            Write-Error "❌ $serviceName : UNHEALTHY"
        }
    }
    
    Write-Output ""
    $healthRate = if ($totalServices -gt 0) { [math]::Round(($healthyServices / $totalServices) * 100, 1) } else { 0 }
    
    if ($healthRate -eq 100) {
        Write-Success "🎉 TOUS LES SERVICES SONT HEALTHY: $healthyServices/$totalServices ($healthRate%)"
    } elseif ($healthRate -ge 75) {
        Write-Warning "⚠️  MAJORITÉ DES SERVICES OK: $healthyServices/$totalServices ($healthRate%)"
    } else {
        Write-Error "❌ PROBLÈMES CRITIQUES: $healthyServices/$totalServices ($healthRate%)"
    }
    
    Write-Info "==============================================="
}

# SCRIPT PRINCIPAL
# ===============================================

try {
    Show-Header
    
    if ($CheckHealth) {
        $results = Test-AllServices
        Show-ServicesSummary $results
    }
    
    if ($StartServices) {
        Write-Info "🚀 Démarrage de tous les services..."
        Write-Output ""
        
        $runningServices = @{}
        
        foreach ($serviceName in $services.Keys) {
            $result = Start-Service $serviceName $services[$serviceName]
            if ($result.Success) {
                $runningServices[$serviceName] = $result
            }
            Write-Output ""
        }
        
        if ($runningServices.Count -gt 0) {
            Write-Success "✅ Services démarrés avec succès: $($runningServices.Keys -join ', ')"
            Write-Info "💡 Utilisez Ctrl+C pour arrêter les services"
            Write-Info "💡 Ou exécutez ce script avec -StopServices pour les arrêter"
            
            # Garder le script en vie
            try {
                while ($true) {
                    Start-Sleep -Seconds 5
                    
                    # Vérifier que les services tournent toujours
                    foreach ($serviceName in $runningServices.Keys) {
                        $process = $runningServices[$serviceName].Process
                        if ($process.HasExited) {
                            Write-Warning "⚠️  Service $serviceName s'est arrêté"
                            $runningServices.Remove($serviceName)
                        }
                    }
                    
                    if ($runningServices.Count -eq 0) {
                        Write-Info "ℹ️  Tous les services se sont arrêtés"
                        break
                    }
                }
            } catch {
                Write-Info "ℹ️  Arrêt des services..."
                foreach ($serviceName in $runningServices.Keys) {
                    $process = $runningServices[$serviceName].Process
                    if (-not $process.HasExited) {
                        $process.Kill()
                        Write-Info "🛑 Service $serviceName arrêté"
                    }
                }
            }
        }
    }
    
    if ($StopServices) {
        Write-Info "🛑 Arrêt de tous les services NiesPro..."
        
        # Trouver tous les processus dotnet qui pourraient être nos services
        $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
        
        foreach ($process in $dotnetProcesses) {
            # Logique pour identifier nos services (par exemple, par port ou nom)
            try {
                $process.Kill()
                Write-Info "🛑 Processus dotnet arrêté (PID: $($process.Id))"
            } catch {
                Write-Warning "⚠️  Impossible d'arrêter le processus $($process.Id)"
            }
        }
        
        Write-Success "✅ Arrêt des services terminé"
    }
    
} catch {
    Write-Error "❌ Erreur durant les tests: $($_.Exception.Message)"
    exit 1
}

Write-Output ""
Write-Info "Tests terminés. Appuyez sur une touche pour continuer..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")