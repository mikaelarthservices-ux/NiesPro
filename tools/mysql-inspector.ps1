# =======================================================================
# MYSQL DATABASE INSPECTOR - NiesPro
# Script simple pour consulter toutes les bases MySQL
# =======================================================================

param(
    [string]$Host = "localhost",
    [string]$Port = "3306",
    [string]$User = "root",
    [string]$Password = "",
    [string]$Database = ""
)

Write-Host "🗄️  MYSQL DATABASE INSPECTOR" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

# Fonction pour trouver mysql.exe
function Find-MySqlExecutable {
    $paths = @(
        "mysql",
        "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
        "C:\Program Files\MySQL\MySQL Server 8.1\bin\mysql.exe", 
        "C:\wamp64\bin\mysql\mysql8.0.31\bin\mysql.exe",
        "C:\wamp64\bin\mysql\mysql8.1.0\bin\mysql.exe",
        "C:\wamp\bin\mysql\mysql8.0.31\bin\mysql.exe",
        "C:\xampp\mysql\bin\mysql.exe",
        "C:\laragon\bin\mysql\mysql-8.0.30-winx64\bin\mysql.exe"
    )
    
    foreach ($path in $paths) {
        $cmd = Get-Command $path -ErrorAction SilentlyContinue
        if ($cmd) {
            Write-Host "✅ MySQL trouvé: $($cmd.Source)" -ForegroundColor Green
            return $cmd.Source
        }
    }
    
    Write-Host "❌ MySQL client non trouvé dans les chemins standards" -ForegroundColor Red
    Write-Host "💡 Chemins recherchés:" -ForegroundColor Yellow
    $paths | ForEach-Object { Write-Host "   - $_" -ForegroundColor Gray }
    return $null
}

# Fonction pour exécuter une requête
function Invoke-MySqlQuery {
    param([string]$Query, [string]$TargetDatabase = "")
    
    $mysql = Find-MySqlExecutable
    if (-not $mysql) {
        return $null
    }
    
    try {
        $args = @("-h", $Host, "-P", $Port, "-u", $User)
        
        if ($Password) {
            $args += "-p$Password"
        }
        
        if ($TargetDatabase) {
            $args += $TargetDatabase
        }
        
        $args += "-e"
        $args += $Query
        
        Write-Host "🔍 Exécution: $Query" -ForegroundColor Cyan
        $result = & $mysql @args 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            return $result
        } else {
            Write-Host "❌ Erreur SQL: $result" -ForegroundColor Red
            return $null
        }
    }
    catch {
        Write-Host "❌ Erreur: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Test de connexion
Write-Host "🔌 Test de connexion MySQL..." -ForegroundColor Yellow
$testResult = Invoke-MySqlQuery "SELECT 'Connexion OK' AS Status, NOW() AS Timestamp, VERSION() AS Version;"

if (-not $testResult) {
    Write-Host "❌ Impossible de se connecter à MySQL" -ForegroundColor Red
    Write-Host "💡 Vérifiez:" -ForegroundColor Yellow
    Write-Host "   - MySQL est démarré" -ForegroundColor Gray
    Write-Host "   - Host: $Host" -ForegroundColor Gray
    Write-Host "   - Port: $Port" -ForegroundColor Gray
    Write-Host "   - User: $User" -ForegroundColor Gray
    exit 1
}

Write-Host "✅ Connexion MySQL réussie!" -ForegroundColor Green
Write-Host $testResult -ForegroundColor White

# Lister toutes les bases de données
Write-Host "`n📚 BASES DE DONNÉES DISPONIBLES:" -ForegroundColor Yellow
$databases = Invoke-MySqlQuery "SHOW DATABASES;"
if ($databases) {
    Write-Host $databases -ForegroundColor White
}

# Lister spécifiquement les bases NiesPro
Write-Host "`n🎯 BASES NIESPRO:" -ForegroundColor Yellow
$niesproDBs = Invoke-MySqlQuery "SHOW DATABASES LIKE 'NiesPro_%' OR SHOW DATABASES LIKE 'niespro_%';"
if ($niesproDBs) {
    Write-Host $niesproDBs -ForegroundColor Cyan
} else {
    Write-Host "❌ Aucune base NiesPro trouvée" -ForegroundColor Red
}

# Si une base spécifique est demandée, l'analyser
if ($Database) {
    Write-Host "`n🔍 ANALYSE DE LA BASE: $Database" -ForegroundColor Magenta
    
    # Vérifier que la base existe
    $dbExists = Invoke-MySqlQuery "SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME='$Database';"
    if (-not $dbExists) {
        Write-Host "❌ La base '$Database' n'existe pas" -ForegroundColor Red
        exit 1
    }
    
    # Lister les tables
    Write-Host "📋 Tables dans $Database :" -ForegroundColor Yellow
    $tables = Invoke-MySqlQuery "SHOW TABLES;" -TargetDatabase $Database
    if ($tables) {
        Write-Host $tables -ForegroundColor White
        
        # Compter les enregistrements de chaque table
        Write-Host "`n📊 NOMBRE D'ENREGISTREMENTS:" -ForegroundColor Yellow
        $tableNames = ($tables -split "`n" | Select-Object -Skip 1).Trim()
        
        foreach ($tableName in $tableNames) {
            if ($tableName -and $tableName -ne "") {
                $count = Invoke-MySqlQuery "SELECT COUNT(*) as Count FROM $tableName;" -TargetDatabase $Database
                if ($count) {
                    $countValue = ($count -split "`n" | Select-Object -Skip 1).Trim()
                    Write-Host "$tableName : $countValue enregistrements" -ForegroundColor Cyan
                }
            }
        }
    } else {
        Write-Host "❌ Aucune table trouvée ou base vide" -ForegroundColor Red
    }
}

# Menu interactif
Write-Host "`n🎯 ACTIONS DISPONIBLES:" -ForegroundColor Yellow
Write-Host "1. Analyser une base spécifique" -ForegroundColor White
Write-Host "2. Créer la base niespro_catalog_dev" -ForegroundColor White
Write-Host "3. Créer toutes les bases NiesPro" -ForegroundColor White
Write-Host "4. Requête personnalisée" -ForegroundColor White
Write-Host "5. Quitter" -ForegroundColor White

$choice = Read-Host "`nVotre choix"

switch ($choice) {
    "1" {
        $targetDB = Read-Host "Nom de la base à analyser"
        Write-Host "`n🔄 Relancement avec la base $targetDB..." -ForegroundColor Green
        & $PSCommandPath -Host $Host -Port $Port -User $User -Password $Password -Database $targetDB
    }
    "2" {
        Write-Host "`n🔧 Création de la base niespro_catalog_dev..." -ForegroundColor Yellow
        $createDB = Invoke-MySqlQuery "CREATE DATABASE IF NOT EXISTS niespro_catalog_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
        if ($createDB -ne $null) {
            Write-Host "✅ Base niespro_catalog_dev créée!" -ForegroundColor Green
        }
    }
    "3" {
        Write-Host "`n🔧 Création de toutes les bases NiesPro..." -ForegroundColor Yellow
        $basesToCreate = @(
            "NiesPro_Auth",
            "niespro_catalog_dev", 
            "NiesPro_Order_Dev",
            "NiesPro_Payment_Dev",
            "NiesPro_Restaurant_Dev"
        )
        
        foreach ($db in $basesToCreate) {
            $result = Invoke-MySqlQuery "CREATE DATABASE IF NOT EXISTS $db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
            if ($result -ne $null) {
                Write-Host "✅ Base $db créée/vérifiée" -ForegroundColor Green
            }
        }
    }
    "4" {
        $customQuery = Read-Host "Entrez votre requête SQL"
        $customDB = Read-Host "Base de données (optionnel)"
        $result = Invoke-MySqlQuery $customQuery -TargetDatabase $customDB
        if ($result) {
            Write-Host $result -ForegroundColor White
        }
    }
    "5" {
        Write-Host "👋 Au revoir!" -ForegroundColor Green
    }
    default {
        Write-Host "❌ Choix invalide" -ForegroundColor Red
    }
}

Write-Host "`n📋 RÉSUMÉ DE LA SESSION:" -ForegroundColor Green
Write-Host "- Host: $Host`:$Port" -ForegroundColor Gray
Write-Host "- User: $User" -ForegroundColor Gray
Write-Host "- Base analysée: $Database" -ForegroundColor Gray
Write-Host "- Timestamp: $(Get-Date)" -ForegroundColor Gray