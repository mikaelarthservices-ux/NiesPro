# ===============================================
# Script de Détection MySQL et Diagnostic - NiesPro
# ===============================================

function Find-MySQL {
    $possiblePaths = @(
        "mysql",
        "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
        "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe",
        "C:\MySQL\bin\mysql.exe",
        "C:\xampp\mysql\bin\mysql.exe"
    )
    
    foreach ($path in $possiblePaths) {
        try {
            $result = & $path --version 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "MySQL trouvé: $path" -ForegroundColor Green
                return $path
            }
        }
        catch {
            # Continue la recherche
        }
    }
    
    return $null
}

function Test-MySQLWithDotNet {
    Write-Host "Test via Entity Framework..." -ForegroundColor Yellow
    
    # Test avec Customer.API qui fonctionne
    try {
        Push-Location "C:\Users\HP\Documents\projets\NiesPro\src\Services\Customer.API"
        $result = & dotnet ef database list 2>$null
        Pop-Location
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Connexion MySQL via EF Core: OK" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Pop-Location
    }
    
    return $false
}

function Show-MySQLStatus {
    Write-Host "=== DIAGNOSTIC MYSQL ===" -ForegroundColor White
    
    # Test du port 3306
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", 3306)
        $connection.Close()
        Write-Host "Port 3306: OUVERT" -ForegroundColor Green
    }
    catch {
        Write-Host "Port 3306: FERME" -ForegroundColor Red
        return $false
    }
    
    # Recherche de MySQL
    $mysqlPath = Find-MySQL
    if ($mysqlPath) {
        try {
            $result = & $mysqlPath -u root -e "SHOW DATABASES;" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Connexion MySQL CLI: OK" -ForegroundColor Green
                Write-Host "Bases disponibles:" -ForegroundColor Gray
                $result | Where-Object { $_ -and $_ -ne "Database" } | ForEach-Object {
                    Write-Host "  - $_" -ForegroundColor Gray
                }
                return $true
            }
        }
        catch {
            Write-Host "Erreur avec MySQL CLI" -ForegroundColor Red
        }
    } else {
        Write-Host "MySQL CLI: NON TROUVE" -ForegroundColor Yellow
    }
    
    # Test via .NET
    $dotnetOk = Test-MySQLWithDotNet
    return $dotnetOk
}

function Show-ProcessInfo {
    Write-Host "=== PROCESSUS ACTIFS ===" -ForegroundColor White
    
    # Processus MySQL
    $mysqlProcesses = Get-Process | Where-Object { $_.ProcessName -like "*mysql*" }
    if ($mysqlProcesses) {
        Write-Host "Processus MySQL:" -ForegroundColor Green
        $mysqlProcesses | ForEach-Object {
            Write-Host "  - $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray
        }
    } else {
        Write-Host "Aucun processus MySQL trouvé" -ForegroundColor Yellow
    }
    
    # Processus .NET
    $dotnetProcesses = Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" -or $_.ProcessName -like "*.API" }
    if ($dotnetProcesses) {
        Write-Host "Processus .NET/API:" -ForegroundColor Green
        $dotnetProcesses | ForEach-Object {
            Write-Host "  - $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray
        }
    }
}

# Exécution
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "DIAGNOSTIC MYSQL & PROCESSUS - NIESPRO" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

Show-MySQLStatus
Write-Host ""
Show-ProcessInfo

Write-Host ""
Write-Host "Diagnostic terminé: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray