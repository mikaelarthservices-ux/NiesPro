# ===============================================
# Script de Diagnostic des Bases de Données - NiesPro
# ===============================================

param(
    [switch]$ListDatabases,
    [switch]$CheckTables,
    [switch]$CreateMissing,
    [switch]$All
)

$databases = @("NiesPro_Auth", "NiesPro_Payment", "NiesPro_Order", "NiesPro_Catalog", "NiesPro_Customer")

function Write-Header {
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "DIAGNOSTIC DES BASES DE DONNEES - NIESPRO" -ForegroundColor Cyan
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Test-MySQLConnection {
    try {
        # Test avec PowerShell natif
        $result = & mysql -u root -e "SELECT 1;" 2>$null
        return $true
    }
    catch {
        try {
            # Test avec chemin complet si MySQL dans PATH
            $result = & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -e "SELECT 1;" 2>$null
            return $true
        }
        catch {
            return $false
        }
    }
}

function Get-DatabaseList {
    try {
        $result = & mysql -u root -e "SHOW DATABASES;" 2>$null
        if ($LASTEXITCODE -eq 0) {
            return $result
        } else {
            $result = & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -e "SHOW DATABASES;" 2>$null
            return $result
        }
    }
    catch {
        Write-Host "Erreur lors de la récupération des bases" -ForegroundColor Red
        return $null
    }
}

function Test-DatabaseExists($dbName) {
    try {
        $result = & mysql -u root -e "USE $dbName; SELECT 1;" 2>$null
        return $LASTEXITCODE -eq 0
    }
    catch {
        try {
            $result = & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -e "USE $dbName; SELECT 1;" 2>$null
            return $LASTEXITCODE -eq 0
        }
        catch {
            return $false
        }
    }
}

function Create-Database($dbName) {
    try {
        Write-Host "Création de la base $dbName..." -ForegroundColor Yellow
        $result = & mysql -u root -e "CREATE DATABASE IF NOT EXISTS $dbName;" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  Base $dbName créée avec succès" -ForegroundColor Green
            return $true
        } else {
            $result = & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -e "CREATE DATABASE IF NOT EXISTS $dbName;" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  Base $dbName créée avec succès" -ForegroundColor Green
                return $true
            }
        }
    }
    catch {
        Write-Host "  Erreur lors de la création de $dbName" -ForegroundColor Red
        return $false
    }
}

function Get-TableCount($dbName) {
    try {
        $result = & mysql -u root -e "USE $dbName; SHOW TABLES;" 2>$null
        if ($LASTEXITCODE -eq 0) {
            $tables = $result | Where-Object { $_ -ne "Tables_in_$dbName" -and $_ -ne "" }
            return $tables.Count
        }
        return 0
    }
    catch {
        return 0
    }
}

# Début du diagnostic
Write-Header

# Test de la connexion MySQL
Write-Host "Test de connexion MySQL..." -ForegroundColor Yellow
$mysqlOk = Test-MySQLConnection

if (-not $mysqlOk) {
    Write-Host "ERREUR: Impossible de se connecter à MySQL" -ForegroundColor Red
    Write-Host "Vérifiez que MySQL est démarré et accessible" -ForegroundColor Yellow
    exit 1
}

Write-Host "Connexion MySQL: OK" -ForegroundColor Green
Write-Host ""

if ($ListDatabases -or $All) {
    Write-Host "=== LISTE DES BASES DE DONNEES ===" -ForegroundColor White
    $dbList = Get-DatabaseList
    if ($dbList) {
        $dbList | ForEach-Object { 
            if ($_ -and $_ -ne "Database") {
                Write-Host "  - $_" -ForegroundColor Gray
            }
        }
    }
    Write-Host ""
}

if ($CheckTables -or $All) {
    Write-Host "=== VERIFICATION DES BASES NIESPRO ===" -ForegroundColor White
    foreach ($db in $databases) {
        $exists = Test-DatabaseExists $db
        if ($exists) {
            $tableCount = Get-TableCount $db
            Write-Host "  $db : EXISTE ($tableCount tables)" -ForegroundColor Green
        } else {
            Write-Host "  $db : MANQUANTE" -ForegroundColor Red
            
            if ($CreateMissing) {
                Create-Database $db | Out-Null
            }
        }
    }
    Write-Host ""
}

if ($CreateMissing -and -not ($CheckTables -or $All)) {
    Write-Host "=== CREATION DES BASES MANQUANTES ===" -ForegroundColor White
    foreach ($db in $databases) {
        $exists = Test-DatabaseExists $db
        if (-not $exists) {
            Create-Database $db | Out-Null
        }
    }
}

Write-Host "Diagnostic terminé: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray