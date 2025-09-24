# =============================================================================
# SCRIPT D'AUDIT DES BASES DE DONNÉES MYSQL
# =============================================================================
# Vérifie et audite toutes les bases de données MySQL présentes
# Analyse les tables, utilisateurs, et configurations
# =============================================================================

param(
    [string]$MySqlHost = "localhost",
    [int]$MySqlPort = 3306,
    [string]$AdminUser = "root",
    [string]$AdminPassword = "",
    [switch]$Detailed = $false,
    [switch]$ExportResults = $false
)

# Couleurs pour l'affichage
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"
$White = "White"

# Variables globales
$script:Results = @{}
$script:DatabaseList = @()
$script:Issues = @()

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "=" * 80 -ForegroundColor $Cyan
    Write-Host " $Title" -ForegroundColor $White
    Write-Host "=" * 80 -ForegroundColor $Cyan
    Write-Host ""
}

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "--- $Title ---" -ForegroundColor $Yellow
    Write-Host ""
}

function Test-MySqlConnection {
    param(
        [string]$ServerHost,
        [int]$ServerPort,
        [string]$ServerUser,
        [string]$ServerPassword
    )
    
    try {
        # Test de connexion avec mysql.exe
        $connectionTest = "SELECT 1 as test;"
        $mysqlCmd = "mysql -h$ServerHost -P$ServerPort -u$ServerUser"
        
        if ($ServerPassword) {
            $mysqlCmd += " -p$ServerPassword"
        }
        
        $mysqlCmd += " -e `"$connectionTest`" 2>&1"
        
        $result = Invoke-Expression $mysqlCmd
        
        if ($LASTEXITCODE -eq 0) {
            return $true
        } else {
            Write-Host "Erreur de connexion MySQL: $result" -ForegroundColor $Red
            return $false
        }
    }
    catch {
        Write-Host "Erreur lors du test de connexion: $($_.Exception.Message)" -ForegroundColor $Red
        return $false
    }
}

function Get-MySqlDatabases {
    param(
        [string]$Host,
        [int]$Port,
        [string]$User,
        [string]$Password
    )
    
    try {
        $query = "SHOW DATABASES;"
        $mysqlCmd = "mysql -h$Host -P$Port -u$User"
        
        if ($Password) {
            $mysqlCmd += " -p$Password"
        }
        
        $mysqlCmd += " -e `"$query`" --batch --skip-column-names 2>&1"
        
        $result = Invoke-Expression $mysqlCmd
        
        if ($LASTEXITCODE -eq 0) {
            $databases = $result | Where-Object { $_ -and $_ -notmatch "^Database$" -and $_ -ne "" }
            return $databases
        } else {
            Write-Host "Erreur lors de la récupération des bases: $result" -ForegroundColor $Red
            return @()
        }
    }
    catch {
        Write-Host "Erreur: $($_.Exception.Message)" -ForegroundColor $Red
        return @()
    }
}

function Get-DatabaseTables {
    param(
        [string]$Database,
        [string]$Host,
        [int]$Port,
        [string]$User,
        [string]$Password
    )
    
    try {
        $query = "USE $Database; SHOW TABLES;"
        $mysqlCmd = "mysql -h$Host -P$Port -u$User"
        
        if ($Password) {
            $mysqlCmd += " -p$Password"
        }
        
        $mysqlCmd += " -e `"$query`" --batch --skip-column-names 2>&1"
        
        $result = Invoke-Expression $mysqlCmd
        
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

function Get-TableInfo {
    param(
        [string]$Database,
        [string]$Table,
        [string]$Host,
        [int]$Port,
        [string]$User,
        [string]$Password
    )
    
    try {
        $query = "USE $Database; SELECT COUNT(*) as row_count FROM $Table;"
        $mysqlCmd = "mysql -h$Host -P$Port -u$User"
        
        if ($Password) {
            $mysqlCmd += " -p$Password"
        }
        
        $mysqlCmd += " -e `"$query`" --batch --skip-column-names 2>&1"
        
        $result = Invoke-Expression $mysqlCmd
        
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

function Get-DatabaseUsers {
    param(
        [string]$Host,
        [int]$Port,
        [string]$User,
        [string]$Password
    )
    
    try {
        $query = "SELECT User, Host FROM mysql.user ORDER BY User;"
        $mysqlCmd = "mysql -h$Host -P$Port -u$User"
        
        if ($Password) {
            $mysqlCmd += " -p$Password"
        }
        
        $mysqlCmd += " -e `"$query`" --batch 2>&1"
        
        $result = Invoke-Expression $mysqlCmd
        
        if ($LASTEXITCODE -eq 0) {
            return $result
        } else {
            return @()
        }
    }
    catch {
        return @()
    }
}

function Analyze-Database {
    param(
        [string]$Database,
        [string]$Host,
        [int]$Port,
        [string]$User,
        [string]$Password,
        [bool]$DetailedAnalysis
    )
    
    $analysis = @{
        Name = $Database
        Tables = @()
        TotalRows = 0
        IsSystemDatabase = $false
        HasData = $false
        PotentialIssues = @()
    }
    
    # Vérifier si c'est une base système
    $systemDbs = @('information_schema', 'mysql', 'performance_schema', 'sys')
    $analysis.IsSystemDatabase = $Database -in $systemDbs
    
    # Récupérer les tables
    $tables = Get-DatabaseTables -Database $Database -Host $Host -Port $Port -User $User -Password $Password
    
    foreach ($table in $tables) {
        if ($table -and $table.Trim() -ne "") {
            $tableInfo = @{
                Name = $table.Trim()
                RowCount = 0
            }
            
            if ($DetailedAnalysis) {
                $rowCount = Get-TableInfo -Database $Database -Table $table.Trim() -Host $Host -Port $Port -User $User -Password $Password
                $tableInfo.RowCount = $rowCount
                $analysis.TotalRows += $rowCount
                
                if ($rowCount -gt 0) {
                    $analysis.HasData = $true
                }
            }
            
            $analysis.Tables += $tableInfo
        }
    }
    
    # Analyser les problèmes potentiels
    if (-not $analysis.IsSystemDatabase) {
        if ($analysis.Tables.Count -eq 0) {
            $analysis.PotentialIssues += "Base de données vide (aucune table)"
        }
        
        if ($DetailedAnalysis -and $analysis.Tables.Count -gt 0 -and -not $analysis.HasData) {
            $analysis.PotentialIssues += "Tables présentes mais aucune donnée"
        }
        
        # Vérifier les noms suspects
        if ($Database -match 'test|temp|backup|old') {
            $analysis.PotentialIssues += "Nom de base suspect (test/temp/backup)"
        }
    }
    
    return $analysis
}

function Show-DatabaseSummary {
    param([array]$Analyses)
    
    Write-Section "RÉSUMÉ DES BASES DE DONNÉES"
    
    $userDatabases = $Analyses | Where-Object { -not $_.IsSystemDatabase }
    $systemDatabases = $Analyses | Where-Object { $_.IsSystemDatabase }
    
    Write-Host "Total des bases de données: $($Analyses.Count)" -ForegroundColor $White
    Write-Host "  - Bases système: $($systemDatabases.Count)" -ForegroundColor $Cyan
    Write-Host "  - Bases utilisateur: $($userDatabases.Count)" -ForegroundColor $Green
    Write-Host ""
    
    if ($userDatabases.Count -gt 0) {
        Write-Host "BASES DE DONNÉES UTILISATEUR:" -ForegroundColor $Green
        foreach ($db in $userDatabases) {
            $status = if ($db.HasData) { "[OK] Avec donnees" } else { "[WARN] Vide" }
            $color = if ($db.HasData) { $Green } else { $Yellow }
            
            Write-Host "  $($db.Name) - $($db.Tables.Count) tables - $status" -ForegroundColor $color
            
            if ($db.PotentialIssues.Count -gt 0) {
                foreach ($issue in $db.PotentialIssues) {
                    Write-Host "    [WARN] $issue" -ForegroundColor $Yellow
                }
            }
        }
    }
    
    Write-Host ""
    Write-Host "BASES DE DONNÉES SYSTÈME:" -ForegroundColor $Cyan
    foreach ($db in $systemDatabases) {
        Write-Host "  $($db.Name) - $($db.Tables.Count) tables" -ForegroundColor $Cyan
    }
}

function Show-DetailedAnalysis {
    param([array]$Analyses)
    
    Write-Section "ANALYSE DÉTAILLÉE"
    
    $userDatabases = $Analyses | Where-Object { -not $_.IsSystemDatabase }
    
    foreach ($db in $userDatabases) {
        Write-Host ""
        Write-Host "Base: $($db.Name)" -ForegroundColor $White
        Write-Host "Tables: $($db.Tables.Count), Lignes totales: $($db.TotalRows)" -ForegroundColor $Cyan
        
        if ($db.Tables.Count -gt 0) {
            Write-Host "  Tables:"
            foreach ($table in $db.Tables) {
                $rowInfo = if ($table.RowCount -gt 0) { " ($($table.RowCount) rows)" } else { " (empty)" }
                Write-Host "    - $($table.Name)$rowInfo" -ForegroundColor $White
            }
        }
        
        if ($db.PotentialIssues.Count -gt 0) {
            Write-Host "  Problemes detectes:" -ForegroundColor $Yellow
            foreach ($issue in $db.PotentialIssues) {
                Write-Host "    [WARN] $issue" -ForegroundColor $Yellow
            }
        }
    }
}

function Export-Results {
    param([array]$Analyses, [string]$FilePath)
    
    $report = @{
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        TotalDatabases = $Analyses.Count
        UserDatabases = ($Analyses | Where-Object { -not $_.IsSystemDatabase }).Count
        SystemDatabases = ($Analyses | Where-Object { $_.IsSystemDatabase }).Count
        Databases = $Analyses
    }
    
    $jsonReport = $report | ConvertTo-Json -Depth 4
    $jsonReport | Out-File -FilePath $FilePath -Encoding UTF8
    
    Write-Host "Rapport exporté vers: $FilePath" -ForegroundColor $Green
}

# =============================================================================
# SCRIPT PRINCIPAL
# =============================================================================

Write-Header "AUDIT DES BASES DE DONNÉES MYSQL"

Write-Host "Configuration:" -ForegroundColor $White
Write-Host "  Serveur: ${MySqlHost}:${MySqlPort}" -ForegroundColor $Cyan
Write-Host "  Utilisateur: $AdminUser" -ForegroundColor $Cyan
Write-Host "  Analyse détaillée: $Detailed" -ForegroundColor $Cyan
Write-Host ""

# Test de connexion
Write-Section "TEST DE CONNEXION"
if (-not (Test-MySqlConnection -Host $MySqlHost -Port $MySqlPort -User $AdminUser -Password $AdminPassword)) {
    Write-Host "[ERROR] Impossible de se connecter a MySQL" -ForegroundColor $Red
    Write-Host "Vérifiez que:" -ForegroundColor $Yellow
    Write-Host "  - MySQL est démarré" -ForegroundColor $Yellow
    Write-Host "  - Les paramètres de connexion sont corrects" -ForegroundColor $Yellow
    Write-Host "  - L'utilisateur a les permissions nécessaires" -ForegroundColor $Yellow
    exit 1
}

Write-Host "[OK] Connexion MySQL reussie" -ForegroundColor $Green

# Récupération des bases de données
Write-Section "RÉCUPÉRATION DES BASES DE DONNÉES"
$databases = Get-MySqlDatabases -Host $MySqlHost -Port $MySqlPort -User $AdminUser -Password $AdminPassword

if ($databases.Count -eq 0) {
    Write-Host "[ERROR] Aucune base de donnees trouvee" -ForegroundColor $Red
    exit 1
}

Write-Host "[OK] $($databases.Count) bases de donnees trouvees" -ForegroundColor $Green

# Analyse des bases de données
Write-Section "ANALYSE DES BASES DE DONNÉES"
$analyses = @()

foreach ($db in $databases) {
    if ($db -and $db.Trim() -ne "") {
        Write-Host "Analyse de: $($db.Trim())..." -ForegroundColor $Cyan
        $analysis = Analyze-Database -Database $db.Trim() -Host $MySqlHost -Port $MySqlPort -User $AdminUser -Password $AdminPassword -DetailedAnalysis $Detailed
        $analyses += $analysis
    }
}

# Affichage des résultats
Show-DatabaseSummary -Analyses $analyses

if ($Detailed) {
    Show-DetailedAnalysis -Analyses $analyses
}

# Récupération des utilisateurs MySQL
Write-Section "UTILISATEURS MYSQL"
$users = Get-DatabaseUsers -Host $MySqlHost -Port $MySqlPort -User $AdminUser -Password $AdminPassword
if ($users.Count -gt 0) {
    Write-Host $users -ForegroundColor $Cyan
} else {
    Write-Host "Impossible de récupérer la liste des utilisateurs" -ForegroundColor $Yellow
}

# Export des résultats
if ($ExportResults) {
    $exportPath = "C:\Users\HP\Documents\projets\NiesPro\tests\Reports\mysql-audit-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    Export-Results -Analyses $analyses -FilePath $exportPath
}

Write-Header "AUDIT TERMINÉ"

$userDatabases = $analyses | Where-Object { -not $_.IsSystemDatabase }
$issuesCount = ($userDatabases | ForEach-Object { $_.PotentialIssues.Count } | Measure-Object -Sum).Sum

Write-Host "Résumé de l'audit:" -ForegroundColor $White
Write-Host "  - Bases utilisateur analysées: $($userDatabases.Count)" -ForegroundColor $Green
Write-Host "  - Problèmes détectés: $issuesCount" -ForegroundColor $(if ($issuesCount -gt 0) { $Yellow } else { $Green })

if ($issuesCount -gt 0) {
    Write-Host ""
    Write-Host "[WARN] Des problemes ont ete detectes. Consultez l'analyse detaillee ci-dessus." -ForegroundColor $Yellow
}

Write-Host ""
Write-Host "Pour une analyse plus détaillée, utilisez le paramètre -Detailed" -ForegroundColor $Cyan
Write-Host "Pour exporter les résultats, utilisez le paramètre -ExportResults" -ForegroundColor $Cyan