# Script pour arreter tous les services NiesPro
Write-Host "Arret de tous les services NiesPro..." -ForegroundColor Yellow

# Fonction pour arreter un processus specifique par port
function Stop-ServiceByPort {
    param([int]$Port, [string]$ServiceName)
    
    try {
        # Trouver le processus qui utilise le port
        $netstat = netstat -ano | Select-String ":$Port "
        
        if ($netstat) {
            foreach ($line in $netstat) {
                if ($line -match "\s+(\d+)$") {
                    $processId = $matches[1]
                    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
                    
                    if ($process -and $process.ProcessName -eq "dotnet") {
                        Write-Host "Arret du service $ServiceName (PID: $processId, Port: $Port)" -ForegroundColor Red
                        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                        Start-Sleep -Seconds 1
                    }
                }
            }
        }
        else {
            Write-Host "Service $ServiceName (Port: $Port) deja arrete" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "Erreur lors de l'arret de $ServiceName : $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Arrêter chaque service par son port
Stop-ServiceByPort -Port 5000 -ServiceName "Gateway API"
Stop-ServiceByPort -Port 5001 -ServiceName "Auth API"
Stop-ServiceByPort -Port 5002 -ServiceName "Order API"
Stop-ServiceByPort -Port 5003 -ServiceName "Catalog API"
Stop-ServiceByPort -Port 5004 -ServiceName "Payment API"

# Arret final de tous les processus dotnet restants
Write-Host "Nettoyage final des processus dotnet..." -ForegroundColor Yellow
try {
    $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    if ($dotnetProcesses) {
        $dotnetProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
        Write-Host "Tous les processus dotnet ont ete arretes" -ForegroundColor Green
    }
    else {
        Write-Host "Aucun processus dotnet en cours" -ForegroundColor Gray
    }
}
catch {
    Write-Host "Erreur lors du nettoyage : $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Arret des services termine" -ForegroundColor Green