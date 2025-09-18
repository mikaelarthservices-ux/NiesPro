# Script pour redemarrer tous les services NiesPro en arriere-plan
param(
    [switch]$Silent = $false
)

if (-not $Silent) {
    Write-Host "Redemarrage complet des services NiesPro..." -ForegroundColor Cyan
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

# Fonction pour arreter un service
function Stop-ServiceByPort {
    param([int]$Port, [string]$ServiceName)
    
    try {
        $netstat = netstat -ano | Select-String ":$Port "
        if ($netstat) {
            foreach ($line in $netstat) {
                if ($line -match "\s+(\d+)$") {
                    $processId = $matches[1]
                    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
                    if ($process -and $process.ProcessName -eq "dotnet") {
                        if (-not $Silent) {
                            Write-Host "Arret du service $ServiceName (PID: $processId)" -ForegroundColor Red
                        }
                        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                    }
                }
            }
        }
    }
    catch {
        if (-not $Silent) {
            Write-Host "Erreur lors de l'arret de $ServiceName : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Fonction pour demarrer un service en arriere-plan
function Start-ServiceBackground {
    param($ServiceInfo)
    
    try {
        if (Test-Path $ServiceInfo.Project) {
            if (-not $Silent) {
                Write-Host "Demarrage de $($ServiceInfo.Name)..." -ForegroundColor Green
            }
            
            $startInfo = New-Object System.Diagnostics.ProcessStartInfo
            $startInfo.FileName = "dotnet"
            $startInfo.Arguments = "run --project `"$($ServiceInfo.Project)`" --urls `"$($ServiceInfo.Url)`""
            $startInfo.UseShellExecute = $false
            $startInfo.CreateNoWindow = $true
            $startInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
            $startInfo.RedirectStandardOutput = $true
            $startInfo.RedirectStandardError = $true
            
            $process = [System.Diagnostics.Process]::Start($startInfo)
            Start-Sleep -Seconds 1
            
            if (-not $Silent) {
                Write-Host "$($ServiceInfo.Name) demarre en arriere-plan sur $($ServiceInfo.Url)" -ForegroundColor Green
            }
        }
        else {
            if (-not $Silent) {
                Write-Host "Projet non trouve: $($ServiceInfo.Project)" -ForegroundColor Red
            }
        }
    }
    catch {
        if (-not $Silent) {
            Write-Host "Erreur lors du demarrage de $($ServiceInfo.Name): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Etape 1: Arreter tous les services
if (-not $Silent) {
    Write-Host "=== ARRET DES SERVICES ===" -ForegroundColor Yellow
}
foreach ($service in $serviceProjects) {
    Stop-ServiceByPort -Port $service.Port -ServiceName $service.Name
}

# Nettoyage final
taskkill /f /im dotnet.exe 2>$null
Start-Sleep -Seconds 3

# Etape 2: Redemarrer tous les services en arriere-plan
if (-not $Silent) {
    Write-Host "=== DEMARRAGE DES SERVICES EN ARRIERE-PLAN ===" -ForegroundColor Yellow
}
foreach ($service in $serviceProjects) {
    Start-ServiceBackground -ServiceInfo $service
    Start-Sleep -Seconds 2
}

if (-not $Silent) {
    Write-Host "=== REDEMARRAGE TERMINE ===" -ForegroundColor Green
    Write-Host "Tous les services fonctionnent maintenant en arriere-plan" -ForegroundColor Cyan
}