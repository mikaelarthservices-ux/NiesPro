# =============================================================================
# AUDIT COMPLET DES BASES DE DONNÉES MYSQL NIESPRO
# =============================================================================

param(
    [switch]$ShowData = $false,
    [switch]$ExportReport = $false
)

$mysqlClient = "C:\wamp64\bin\mysql\mysql9.1.0\bin\mysql.exe"
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"
$White = "White"

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor $Cyan
    Write-Host " $Title" -ForegroundColor $White
    Write-Host ("=" * 70) -ForegroundColor $Cyan
    Write-Host ""
}

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "--- $Title ---" -ForegroundColor $Yellow
}

function Get-DatabaseInfo {
    param([string]$DatabaseName)
    
    try {
        # Récupérer les tables
        $tablesResult = & $mysqlClient -uroot -hlocalhost -e "USE $DatabaseName; SHOW TABLES;" 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            return $null
        }
        
        # Parser les tables
        $tables = @()
        $tablesResult | ForEach-Object {
            if ($_ -and $_ -notmatch "Tables_in_" -and $_ -notmatch "^\+.*\+$" -and $_ -notmatch "^\|.*\|$" -and $_ -ne "") {
                $tableName = $_.Trim()
                if ($tableName -and $tableName -ne "") {
                    $tables += $tableName
                }
            }
        }
        
        # Compter les lignes pour chaque table
        $tableDetails = @()
        foreach ($table in $tables) {
            try {
                $countResult = & $mysqlClient -uroot -hlocalhost -e "USE $DatabaseName; SELECT COUNT(*) FROM $table;" --batch --skip-column-names 2>&1
                
                if ($LASTEXITCODE -eq 0 -and $countResult -match '^\d+$') {
                    $rowCount = [int]$countResult
                } else {
                    $rowCount = 0
                }
                
                $tableDetails += @{
                    Name = $table
                    RowCount = $rowCount
                }
            }
            catch {
                $tableDetails += @{
                    Name = $table
                    RowCount = 0
                }
            }
        }
        
        # Informations générales de la base
        $totalRows = ($tableDetails | Measure-Object -Property RowCount -Sum).Sum
        
        return @{
            Name = $DatabaseName
            Tables = $tableDetails
            TableCount = $tables.Count
            TotalRows = $totalRows
            HasData = $totalRows -gt 0
        }
    }
    catch {
        return $null
    }
}

function Get-MigrationInfo {
    param([string]$DatabaseName)
    
    try {
        $migrationResult = & $mysqlClient -uroot -hlocalhost -e "USE $DatabaseName; SELECT MigrationId, ProductVersion FROM __efmigrationshistory ORDER BY MigrationId;" --batch --skip-column-names 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            $migrations = @()
            $migrationResult | ForEach-Object {
                if ($_ -and $_ -ne "" -and $_ -notmatch "^ERROR") {
                    $parts = $_ -split "`t"
                    if ($parts.Count -eq 2) {
                        $migrations += @{
                            MigrationId = $parts[0]
                            ProductVersion = $parts[1]
                        }
                    }
                }
            }
            return $migrations
        } else {
            return @()
        }
    }
    catch {
        return @()
    }
}

# =============================================================================
# SCRIPT PRINCIPAL
# =============================================================================

Write-Header "AUDIT DETAILLE - BASES DE DONNEES NIESPRO"

# Vérification du client MySQL
if (-not (Test-Path $mysqlClient)) {
    Write-Host "[ERROR] Client MySQL non trouvé: $mysqlClient" -ForegroundColor $Red
    exit 1
}

Write-Host "[OK] Client MySQL: $mysqlClient" -ForegroundColor $Green

# Récupération de toutes les bases
Write-Section "DETECTION DES BASES DE DONNEES"

$allDatabases = & $mysqlClient -uroot -hlocalhost -e "SHOW DATABASES;" --batch --skip-column-names 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Impossible de récupérer les bases: $allDatabases" -ForegroundColor $Red
    exit 1
}

# Parser les bases
$databases = @()
$allDatabases | ForEach-Object {
    if ($_ -and $_ -ne "" -and $_ -ne "Database") {
        $databases += $_.Trim()
    }
}

$systemDbs = @('information_schema', 'mysql', 'performance_schema', 'sys')
$userDatabases = $databases | Where-Object { $_ -notin $systemDbs }
$niesproDatabases = $userDatabases | Where-Object { $_ -like "niespro*" }

Write-Host "Total bases: $($databases.Count)" -ForegroundColor $White
Write-Host "Bases utilisateur: $($userDatabases.Count)" -ForegroundColor $Cyan
Write-Host "Bases NiesPro: $($niesproDatabases.Count)" -ForegroundColor $Green

# Analyse des bases NiesPro
Write-Section "ANALYSE DES BASES NIESPRO"

$serviceMappings = @{
    'niespro_auth' = 'Auth.API'
    'niespro_customer' = 'Customer.API' 
    'niespro_catalog' = 'Catalog.API'
    'niespro_order_dev' = 'Order.API'
    'niespro_payment_dev' = 'Payment.API'
}

$analysisResults = @()

foreach ($db in $niesproDatabases) {
    Write-Host ""
    Write-Host "Base: $db" -ForegroundColor $White
    
    $serviceName = $serviceMappings[$db]
    if ($serviceName) {
        Write-Host "Service: $serviceName" -ForegroundColor $Cyan
    } else {
        Write-Host "Service: [INCONNU]" -ForegroundColor $Yellow
    }
    
    $dbInfo = Get-DatabaseInfo -DatabaseName $db
    
    if ($dbInfo) {
        Write-Host "Tables: $($dbInfo.TableCount)" -ForegroundColor $White
        Write-Host "Lignes totales: $($dbInfo.TotalRows)" -ForegroundColor $White
        
        $status = if ($dbInfo.HasData) { "[AVEC DONNEES]" } else { "[VIDE]" }
        $statusColor = if ($dbInfo.HasData) { $Green } else { $Yellow }
        Write-Host "Statut: $status" -ForegroundColor $statusColor
        
        if ($dbInfo.Tables.Count -gt 0) {
            Write-Host "Detail des tables:" -ForegroundColor $Cyan
            foreach ($table in $dbInfo.Tables) {
                $tableStatus = if ($table.RowCount -gt 0) { "[DATA]" } else { "[EMPTY]" }
                $tableColor = if ($table.RowCount -gt 0) { $Green } else { $White }
                Write-Host "  - $($table.Name): $($table.RowCount) lignes $tableStatus" -ForegroundColor $tableColor
            }
        }
        
        # Vérifier les migrations EF
        $migrations = Get-MigrationInfo -DatabaseName $db
        if ($migrations.Count -gt 0) {
            Write-Host "Migrations EF Core: $($migrations.Count)" -ForegroundColor $Green
            if ($ShowData) {
                foreach ($migration in $migrations) {
                    Write-Host "  - $($migration.MigrationId) (EF $($migration.ProductVersion))" -ForegroundColor $Cyan
                }
            }
        } else {
            Write-Host "Migrations EF Core: [AUCUNE]" -ForegroundColor $Yellow
        }
        
        $analysisResults += @{
            Database = $db
            Service = $serviceName
            Info = $dbInfo
            Migrations = $migrations
        }
    } else {
        Write-Host "[ERROR] Impossible d'analyser la base $db" -ForegroundColor $Red
    }
}

# Vérification des services manquants
Write-Section "VERIFICATION DES SERVICES MANQUANTS"

$expectedServices = @(
    @{Service='Auth.API'; Database='niespro_auth'},
    @{Service='Customer.API'; Database='niespro_customer'},
    @{Service='Catalog.API'; Database='niespro_catalog'},
    @{Service='Order.API'; Database='niespro_order*'},
    @{Service='Payment.API'; Database='niespro_payment*'},
    @{Service='Restaurant.API'; Database='niespro_restaurant*'},
    @{Service='Notification.API'; Database='niespro_notification*'}
)

foreach ($expected in $expectedServices) {
    $found = $niesproDatabases | Where-Object { $_ -like $expected.Database }
    
    if ($found) {
        Write-Host "[OK] $($expected.Service) -> $found" -ForegroundColor $Green
    } else {
        Write-Host "[MISSING] $($expected.Service) -> $($expected.Database)" -ForegroundColor $Red
    }
}

# Résumé global
Write-Section "RESUME GLOBAL"

$totalTables = ($analysisResults | ForEach-Object { $_.Info.TableCount } | Measure-Object -Sum).Sum
$totalRows = ($analysisResults | ForEach-Object { $_.Info.TotalRows } | Measure-Object -Sum).Sum
$basesAvecDonnees = ($analysisResults | Where-Object { $_.Info.HasData }).Count

Write-Host "Statistiques NiesPro:" -ForegroundColor $White
Write-Host "  Bases analysées: $($analysisResults.Count)" -ForegroundColor $Cyan
Write-Host "  Tables totales: $totalTables" -ForegroundColor $Cyan
Write-Host "  Lignes totales: $totalRows" -ForegroundColor $Cyan
Write-Host "  Bases avec données: $basesAvecDonnees" -ForegroundColor $Green
Write-Host "  Bases vides: $($analysisResults.Count - $basesAvecDonnees)" -ForegroundColor $Yellow

# Export du rapport
if ($ExportReport) {
    $reportPath = "C:\Users\HP\Documents\projets\NiesPro\tests\Reports\mysql-detailed-audit-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    
    $report = @{
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Summary = @{
            TotalDatabases = $databases.Count
            UserDatabases = $userDatabases.Count  
            NiesproDatabases = $niesproDatabases.Count
            TotalTables = $totalTables
            TotalRows = $totalRows
            BasesWithData = $basesAvecDonnees
        }
        NiesproAnalysis = $analysisResults
        AllDatabases = $databases
    }
    
    $report | ConvertTo-Json -Depth 5 | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host ""
    Write-Host "[INFO] Rapport exporté: $reportPath" -ForegroundColor $Cyan
}

Write-Header "AUDIT TERMINE"

Write-Host "CONCLUSION:" -ForegroundColor $White
Write-Host "[OK] Infrastructure MySQL operationnelle" -ForegroundColor $Green
Write-Host "[OK] $($niesproDatabases.Count) bases NiesPro detectees" -ForegroundColor $Green

if ($basesAvecDonnees -eq 0) {
    Write-Host "[WARN] Toutes les bases sont vides (pas de donnees test)" -ForegroundColor $Yellow
} else {
    Write-Host "[OK] $basesAvecDonnees bases contiennent des donnees" -ForegroundColor $Green
}

$missingServices = $expectedServices | Where-Object { 
    $pattern = $_.Database
    -not ($niesproDatabases | Where-Object { $_ -like $pattern })
}

if ($missingServices.Count -gt 0) {
    Write-Host "[WARN] $($missingServices.Count) services sans base detectee" -ForegroundColor $Yellow
}

Write-Host ""