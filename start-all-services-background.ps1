# Script pour demarrer tous les services NiesPro en arriere-plan (sans fenetres)
param(
    [switch]$Silent = $false
)

if (-not $Silent) {
    Write-Host "Demarrage des services NiesPro en arriere-plan..." -ForegroundColor Green
}

# Definir le repertoire racine du projet
$projectRoot = "c:\Users\HP\Documents\projets\NiesPro"
$serviceProjects = @(
    @{ Name = "Gateway API"; Project = "$projectRoot\src\Gateway\Gateway.API\Gateway.API.csproj"; Port = 5000; Url = "https://localhost:5000" },
    @{ Name = "Auth API"; Project = "$projectRoot\src\Services\Auth\Auth.API\Auth.API.csproj"; Port = 5001; Url = "https://localhost:5001" },
    @{ Name = "Order API"; Project = "$projectRoot\src\Services\Order\Order.API\Order.API.csproj"; Port = 5002; Url = "https://localhost:5002" },
    @{ Name = "Catalog API"; Project = "$projectRoot\src\Services\Catalog\Catalog.API\Catalog.API.csproj"; Port = 5003; Url = "https://localhost:5003" },
    @{ Name = "Payment API"; Project = "$projectRoot\src\Services\Payment\Payment.API\Payment.API.csproj"; Port = 5004; Url = "https://localhost:5004" }
)

# Fonction pour verifier si un port est occupe
function Test-PortInUse {
    param([int]$Port)
    $netstat = netstat -an | Select-String ":$Port " | Select-String "LISTENING"
    return $netstat -ne $null
}

# Fonction pour demarrer un service en arriere-plan
function Start-ServiceBackground {
    param($ServiceInfo)
    
    try {
        # Verifier si le service n'est pas deja en cours
        if (Test-PortInUse -Port $ServiceInfo.Port) {
            if (-not $Silent) {
                Write-Host "$($ServiceInfo.Name) est deja en cours d'execution sur le port $($ServiceInfo.Port)" -ForegroundColor Yellow
            }
            return
        }

        if (Test-Path $ServiceInfo.Project) {
            if (-not $Silent) {
                Write-Host "Demarrage de $($ServiceInfo.Name) en arriere-plan..." -ForegroundColor Green
            }
            
            # Creer le processus avec des parametres pour fonctionner en arriere-plan
            $startInfo = New-Object System.Diagnostics.ProcessStartInfo
            $startInfo.FileName = "dotnet"
            $startInfo.Arguments = "run --project `"$($ServiceInfo.Project)`" --urls `"$($ServiceInfo.Url)`""
            $startInfo.UseShellExecute = $false
            $startInfo.CreateNoWindow = $true
            $startInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
            $startInfo.RedirectStandardOutput = $true
            $startInfo.RedirectStandardError = $true
            
            $process = [System.Diagnostics.Process]::Start($startInfo)
            Start-Sleep -Seconds 2
            
            # Verifier que le service a bien demarre
            if (Test-PortInUse -Port $ServiceInfo.Port) {
                if (-not $Silent) {
                    Write-Host "[OK] $($ServiceInfo.Name) demarre avec succes" -ForegroundColor Green
                }
            }
            else {
                if (-not $Silent) {
                    Write-Host "[...] $($ServiceInfo.Name) : demarrage en cours..." -ForegroundColor Yellow
                }
            }
        }
        else {
            if (-not $Silent) {
                Write-Host "[ERR] Projet non trouve: $($ServiceInfo.Project)" -ForegroundColor Red
            }
        }
    }
    catch {
        if (-not $Silent) {
            Write-Host "[ERR] Erreur lors du demarrage de $($ServiceInfo.Name): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Demarrer tous les services
foreach ($service in $serviceProjects) {
    Start-ServiceBackground -ServiceInfo $service
    Start-Sleep -Seconds 1
}

if (-not $Silent) {
    Write-Host "=== DEMARRAGE TERMINE ===" -ForegroundColor Green
    Write-Host "Tous les services fonctionnent maintenant en arriere-plan" -ForegroundColor Cyan
    Write-Host "Utilisez 'quick-check.ps1' pour verifier l'etat des services" -ForegroundColor Gray
}