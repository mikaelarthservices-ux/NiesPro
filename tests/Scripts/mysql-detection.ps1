# =============================================================================
# AUDIT MYSQL VIA POWERSHELL NATIF
# =============================================================================

param(
    [string]$Server = "localhost",
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

function Test-MySqlConnection {
    try {
        # Test via socket TCP
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect($Server, $Port)
        $connected = $tcpClient.Connected
        $tcpClient.Close()
        return $connected
    }
    catch {
        return $false
    }
}

function Get-DatabasesViaODBC {
    try {
        # Tentative via ODBC si disponible
        $connectionString = "Driver={MySQL ODBC 8.0 ANSI Driver};Server=$Server;Port=$Port;Database=information_schema;"
        if ($Username) { $connectionString += "Uid=$Username;" }
        if ($Password) { $connectionString += "Pwd=$Password;" }
        
        $connection = New-Object System.Data.Odbc.OdbcConnection($connectionString)
        $connection.Open()
        
        $command = New-Object System.Data.Odbc.OdbcCommand("SHOW DATABASES", $connection)
        $reader = $command.ExecuteReader()
        
        $databases = @()
        while ($reader.Read()) {
            $databases += $reader.GetString(0)
        }
        
        $reader.Close()
        $connection.Close()
        
        return $databases
    }
    catch {
        Write-Host "[ERROR] ODBC: $($_.Exception.Message)" -ForegroundColor $Red
        return @()
    }
}

function Invoke-SqlQuery {
    param(
        [string]$Query,
        [string]$Database = "information_schema"
    )
    
    try {
        # Utilisation de mysql via CMD si disponible
        $tempFile = [System.IO.Path]::GetTempFileName()
        $Query | Out-File -FilePath $tempFile -Encoding ASCII
        
        $arguments = @(
            "--host=$Server",
            "--port=$Port",
            "--user=$Username"
        )
        
        if ($Password) {
            $arguments += "--password=$Password"
        }
        
        $arguments += @(
            "--database=$Database",
            "--batch",
            "--skip-column-names",
            "--execute=$Query"
        )
        
        # Recherche de mysql.exe dans les chemins courants
        $mysqlPaths = @(
            "mysql",
            "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
            "C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe",
            "C:\MySQL\bin\mysql.exe",
            "C:\xampp\mysql\bin\mysql.exe",
            "C:\wamp64\bin\mysql\mysql8.0.21\bin\mysql.exe"
        )
        
        foreach ($mysqlPath in $mysqlPaths) {
            try {
                if (Test-Path $mysqlPath -ErrorAction SilentlyContinue) {
                    $result = & $mysqlPath $arguments 2>&1
                    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
                    
                    if ($LASTEXITCODE -eq 0) {
                        return $result
                    }
                }
            }
            catch {
                continue
            }
        }
        
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
        return @()
    }
    catch {
        return @()
    }
}

# =============================================================================
# SCRIPT PRINCIPAL
# =============================================================================

Write-Header "AUDIT MYSQL - DETECTION AUTOMATIQUE"

Write-Host "Configuration:" -ForegroundColor $White
Write-Host "  Serveur: $Server`:$Port" -ForegroundColor $Cyan
Write-Host "  Utilisateur: $Username" -ForegroundColor $Cyan
Write-Host ""

# Test de connexion TCP
Write-Host "Test de connexion TCP..." -ForegroundColor $Cyan
if (Test-MySqlConnection) {
    Write-Host "[OK] MySQL repond sur $Server`:$Port" -ForegroundColor $Green
} else {
    Write-Host "[ERROR] Impossible de se connecter a MySQL sur $Server`:$Port" -ForegroundColor $Red
    exit 1
}

# Méthode 1: Via netstat pour confirmer les informations
Write-Host ""
Write-Host "Verification des ports MySQL via netstat..." -ForegroundColor $Cyan
try {
    $netstatResult = netstat -an | Select-String ":3306"
    if ($netstatResult) {
        Write-Host "[OK] Port 3306 detecte en ecoute:" -ForegroundColor $Green
        $netstatResult | ForEach-Object { Write-Host "  $_" -ForegroundColor $White }
    } else {
        Write-Host "[WARN] Port 3306 non detecte via netstat" -ForegroundColor $Yellow
    }
} catch {
    Write-Host "[ERROR] Erreur netstat: $($_.Exception.Message)" -ForegroundColor $Red
}

# Méthode 2: Via processus MySQL
Write-Host ""
Write-Host "Detection des processus MySQL..." -ForegroundColor $Cyan
try {
    $mysqlProcesses = Get-Process | Where-Object {$_.ProcessName -like "*mysql*"}
    if ($mysqlProcesses) {
        Write-Host "[OK] Processus MySQL detectes:" -ForegroundColor $Green
        $mysqlProcesses | ForEach-Object { 
            Write-Host "  PID: $($_.Id) - Nom: $($_.ProcessName)" -ForegroundColor $White 
        }
    } else {
        Write-Host "[WARN] Aucun processus MySQL detecte" -ForegroundColor $Yellow
    }
} catch {
    Write-Host "[ERROR] Erreur detection processus: $($_.Exception.Message)" -ForegroundColor $Red
}

# Méthode 3: Via services Windows
Write-Host ""
Write-Host "Verification des services MySQL..." -ForegroundColor $Cyan
try {
    $mysqlServices = Get-Service | Where-Object {$_.DisplayName -like "*MySQL*" -or $_.Name -like "*mysql*"}
    if ($mysqlServices) {
        Write-Host "[OK] Services MySQL detectes:" -ForegroundColor $Green
        $mysqlServices | ForEach-Object { 
            $statusColor = if ($_.Status -eq "Running") { $Green } else { $Red }
            Write-Host "  $($_.DisplayName) - Status: $($_.Status)" -ForegroundColor $statusColor
        }
    } else {
        Write-Host "[WARN] Aucun service MySQL detecte" -ForegroundColor $Yellow
    }
} catch {
    Write-Host "[ERROR] Erreur verification services: $($_.Exception.Message)" -ForegroundColor $Red
}

# Tentative de requête SQL
Write-Host ""
Write-Host "Tentative de requete SQL..." -ForegroundColor $Cyan

$databases = Invoke-SqlQuery -Query "SHOW DATABASES"

if ($databases.Count -gt 0) {
    Write-Host "[OK] $($databases.Count) bases de donnees trouvees via SQL" -ForegroundColor $Green
    
    $systemDbs = @('information_schema', 'mysql', 'performance_schema', 'sys')
    $userDatabases = $databases | Where-Object { $_ -notin $systemDbs -and $_ -ne "" }
    $systemDatabases = $databases | Where-Object { $_ -in $systemDbs }
    
    Write-Host ""
    Write-Host "BASES DE DONNEES DETECTEES:" -ForegroundColor $White
    Write-Host "  Total: $($databases.Count)" -ForegroundColor $Cyan
    Write-Host "  Systeme: $($systemDatabases.Count)" -ForegroundColor $Cyan
    Write-Host "  Utilisateur: $($userDatabases.Count)" -ForegroundColor $Green
    
    if ($systemDatabases.Count -gt 0) {
        Write-Host ""
        Write-Host "Bases systeme:" -ForegroundColor $Cyan
        $systemDatabases | ForEach-Object { Write-Host "  - $_" -ForegroundColor $Cyan }
    }
    
    if ($userDatabases.Count -gt 0) {
        Write-Host ""
        Write-Host "Bases utilisateur:" -ForegroundColor $Green
        $userDatabases | ForEach-Object { Write-Host "  - $_" -ForegroundColor $Green }
    } else {
        Write-Host ""
        Write-Host "[INFO] Aucune base utilisateur detectee" -ForegroundColor $Yellow
        Write-Host "       Les microservices n'ont probablement pas encore cree leurs bases" -ForegroundColor $Yellow
    }
} else {
    Write-Host "[ERROR] Impossible d'executer des requetes SQL" -ForegroundColor $Red
    Write-Host "Cela peut indiquer:" -ForegroundColor $Yellow
    Write-Host "  - Probleme d'authentification" -ForegroundColor $Yellow
    Write-Host "  - Client MySQL non disponible" -ForegroundColor $Yellow
    Write-Host "  - Configuration MySQL restrictive" -ForegroundColor $Yellow
}

Write-Host ""
Write-Header "AUDIT TERMINE"

Write-Host "CONCLUSION:" -ForegroundColor $White
Write-Host "- MySQL est actif et ecoute sur le port 3306" -ForegroundColor $Green
if ($databases.Count -gt 0) {
    Write-Host "- Bases de donnees accessibles via requetes SQL" -ForegroundColor $Green
    Write-Host "- Total de $($databases.Count) bases detectees" -ForegroundColor $Green
} else {
    Write-Host "- Bases de donnees non accessibles via SQL (permissions?)" -ForegroundColor $Yellow
}

Write-Host ""
Write-Host "Pour resoudre les problemes d'acces aux donnees:" -ForegroundColor $Cyan
Write-Host "1. Verifiez les permissions MySQL pour l'utilisateur '$Username'" -ForegroundColor $Cyan
Write-Host "2. Testez la connexion avec: mysql -u$Username -p -h$Server" -ForegroundColor $Cyan
Write-Host "3. Verifiez que le client MySQL est installe et accessible" -ForegroundColor $Cyan
Write-Host ""