# =============================================================================
# SCRIPT D'AUDIT SIMPLE DES BASES DE DONNÉES MYSQL
# =============================================================================

param(
    [string]$ServerName = "localhost",
    [int]$Port = 3306,
    [string]$Username = "root",
    [string]$Password = ""
)

$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"
$White = "White"

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host ("=" * 60) -ForegroundColor $Cyan
    Write-Host " $Title" -ForegroundColor $White
    Write-Host ("=" * 60) -ForegroundColor $Cyan
    Write-Host ""
}

function Test-MySqlAvailable {
    try {
        $null = Get-Command mysql -ErrorAction Stop
        return $true
    }
    catch {
        Write-Host "[ERROR] MySQL client non trouve. Verifiez l'installation." -ForegroundColor $Red
        return $false
    }
}

function Get-Databases {
    try {
        $cmd = "mysql -h$ServerName -P$Port -u$Username"
        if ($Password) {
            $cmd += " -p$Password"
        }
        $cmd += " -e `"SHOW DATABASES;`" --batch --skip-column-names 2>&1"
        
        $result = Invoke-Expression $cmd
        
        if ($LASTEXITCODE -eq 0) {
            $databases = $result | Where-Object { $_ -and $_ -ne "Database" -and $_ -ne "" }
            return $databases
        } else {
            Write-Host "[ERROR] Erreur MySQL: $result" -ForegroundColor $Red
            return @()
        }
    }
    catch {
        Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor $Red
        return @()
    }
}

function Get-DatabaseTables {
    param([string]$DatabaseName)
    
    try {
        $cmd = "mysql -h$ServerName -P$Port -u$Username"
        if ($Password) {
            $cmd += " -p$Password"
        }
        $cmd += " -D $DatabaseName -e `"SHOW TABLES;`" --batch --skip-column-names 2>&1"
        
        $result = Invoke-Expression $cmd
        
        if ($LASTEXITCODE -eq 0) {
            $tables = $result | Where-Object { $_ -and $_ -notmatch "^Tables_in_" -and $_ -ne "" }
            return $tables
        } else {
            return @()
        }
    }
    catch {
        return @()
    }
}

function Get-TableRowCount {
    param(
        [string]$DatabaseName,
        [string]$TableName
    )
    
    try {
        $cmd = "mysql -h$ServerName -P$Port -u$Username"
        if ($Password) {
            $cmd += " -p$Password"
        }
        $cmd += " -D $DatabaseName -e `"SELECT COUNT(*) FROM $TableName;`" --batch --skip-column-names 2>&1"
        
        $result = Invoke-Expression $cmd
        
        if ($LASTEXITCODE -eq 0 -and $result -match '^\d+$') {
            return [int]$result
        } else {
            return 0
        }
    }
    catch {
        return 0
    }
}

# =============================================================================
# SCRIPT PRINCIPAL
# =============================================================================

Write-Header "AUDIT DES BASES DE DONNEES MYSQL"

Write-Host "Configuration:" -ForegroundColor $White
Write-Host "  Serveur: $ServerName`:$Port" -ForegroundColor $Cyan
Write-Host "  Utilisateur: $Username" -ForegroundColor $Cyan
Write-Host ""

# Vérification de la disponibilité de MySQL
if (-not (Test-MySqlAvailable)) {
    exit 1
}

Write-Host "[OK] Client MySQL disponible" -ForegroundColor $Green

# Récupération des bases de données
Write-Host ""
Write-Host "Recuperation des bases de donnees..." -ForegroundColor $Cyan
$databases = Get-Databases

if ($databases.Count -eq 0) {
    Write-Host "[ERROR] Aucune base de donnees trouvee ou erreur de connexion" -ForegroundColor $Red
    exit 1
}

Write-Host "[OK] $($databases.Count) bases de donnees trouvees" -ForegroundColor $Green
Write-Host ""

# Classification des bases de données
$systemDbs = @('information_schema', 'mysql', 'performance_schema', 'sys')
$userDatabases = $databases | Where-Object { $_ -notin $systemDbs }
$systemDatabases = $databases | Where-Object { $_ -in $systemDbs }

Write-Host "RESUME:" -ForegroundColor $White
Write-Host "  Total: $($databases.Count) bases" -ForegroundColor $Cyan
Write-Host "  Systeme: $($systemDatabases.Count) bases" -ForegroundColor $Cyan
Write-Host "  Utilisateur: $($userDatabases.Count) bases" -ForegroundColor $Green
Write-Host ""

# Affichage des bases système
if ($systemDatabases.Count -gt 0) {
    Write-Host "BASES DE DONNEES SYSTEME:" -ForegroundColor $Cyan
    foreach ($db in $systemDatabases) {
        Write-Host "  - $db" -ForegroundColor $Cyan
    }
    Write-Host ""
}

# Analyse détaillée des bases utilisateur
if ($userDatabases.Count -gt 0) {
    Write-Host "BASES DE DONNEES UTILISATEUR:" -ForegroundColor $Green
    
    foreach ($db in $userDatabases) {
        Write-Host ""
        Write-Host "Base: $db" -ForegroundColor $White
        
        $tables = Get-DatabaseTables -DatabaseName $db
        
        if ($tables.Count -eq 0) {
            Write-Host "  [WARN] Aucune table trouvee" -ForegroundColor $Yellow
        } else {
            Write-Host "  Tables: $($tables.Count)" -ForegroundColor $Cyan
            
            $totalRows = 0
            foreach ($table in $tables) {
                if ($table -and $table.Trim() -ne "") {
                    $rowCount = Get-TableRowCount -DatabaseName $db -TableName $table.Trim()
                    $totalRows += $rowCount
                    
                    $status = if ($rowCount -gt 0) { "[DATA]" } else { "[EMPTY]" }
                    $color = if ($rowCount -gt 0) { $Green } else { $Yellow }
                    
                    Write-Host "    - $($table.Trim()): $rowCount lignes $status" -ForegroundColor $color
                }
            }
            
            Write-Host "  Total lignes: $totalRows" -ForegroundColor $White
        }
    }
} else {
    Write-Host "AUCUNE BASE DE DONNEES UTILISATEUR TROUVEE" -ForegroundColor $Yellow
}

Write-Host ""
Write-Header "AUDIT TERMINE"

# Résumé final
Write-Host "Resultats de l'audit:" -ForegroundColor $White
Write-Host "  - Bases systeme: $($systemDatabases.Count)" -ForegroundColor $Cyan
Write-Host "  - Bases utilisateur: $($userDatabases.Count)" -ForegroundColor $Green

if ($userDatabases.Count -eq 0) {
    Write-Host ""
    Write-Host "[INFO] Aucune base utilisateur detectee." -ForegroundColor $Yellow
    Write-Host "       Cela peut indiquer que les services n'ont pas encore cree leurs bases." -ForegroundColor $Yellow
}

Write-Host ""