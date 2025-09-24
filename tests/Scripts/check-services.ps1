# ===============================================
# Outil de Vérification des Services - NiesPro
# ===============================================
# Description: Vérification intelligente des services avec PowerShell natif
# ===============================================

param(
    [string]$Service = "all",
    [switch]$Detailed,
    [switch]$Json
)

# Configuration des services
$serviceConfig = @{
    "auth" = @{
        Name = "Auth.API"
        Port = 5001
        ProcessName = @("Auth.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
    "customer" = @{
        Name = "Customer.API"
        Port = 8001
        ProcessName = @("Customer.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
    "catalog" = @{
        Name = "Catalog.API"
        Port = 5003
        ProcessName = @("Catalog.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
    "order" = @{
        Name = "Order.API"
        Port = 5002
        ProcessName = @("Order.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
    "payment" = @{
        Name = "Payment.API"
        Port = 5004
        ProcessName = @("Payment.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
    "gateway" = @{
        Name = "Gateway.API"
        Port = 5000
        ProcessName = @("Gateway.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
    "stock" = @{
        Name = "Stock.API"
        Port = 5005
        ProcessName = @("Stock.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
    "restaurant" = @{
        Name = "Restaurant.API"
        Port = 7001
        ProcessName = @("Restaurant.API", "dotnet")
        HealthEndpoint = "/health"
        SwaggerEndpoint = "/swagger"
    }
}

function Test-ServicePort {
    param([int]$Port, [int]$TimeoutMs = 1000)
    
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $asyncResult = $tcpClient.BeginConnect("127.0.0.1", $Port, $null, $null)
        $waitHandle = $asyncResult.AsyncWaitHandle
        
        if ($waitHandle.WaitOne($TimeoutMs)) {
            $tcpClient.EndConnect($asyncResult)
            $tcpClient.Close()
            return $true
        } else {
            $tcpClient.Close()
            return $false
        }
    }
    catch {
        return $false
    }
}

function Get-ServiceProcess {
    param([array]$ProcessNames, [int]$Port)
    
    # Rechercher par nom de processus
    foreach ($name in $ProcessNames) {
        $processes = Get-Process -Name $name -ErrorAction SilentlyContinue
        if ($processes) {
            foreach ($proc in $processes) {
                # Vérifier si le processus utilise le bon port
                try {
                    $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
                    if ($connections) {
                        foreach ($conn in $connections) {
                            if ($conn.OwningProcess -eq $proc.Id) {
                                return @{
                                    Process = $proc
                                    Connection = $conn
                                    Found = $true
                                }
                            }
                        }
                    }
                }
                catch {
                    # Si Get-NetTCPConnection échoue, retourner quand même le processus
                    return @{
                        Process = $proc
                        Connection = $null
                        Found = $true
                    }
                }
            }
        }
    }
    
    # Si pas trouvé par nom, chercher par port
    try {
        $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
        if ($connections) {
            $conn = $connections[0]
            $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($proc) {
                return @{
                    Process = $proc
                    Connection = $conn
                    Found = $true
                }
            }
        }
    }
    catch {
        # Méthode de fallback avec netstat si Get-NetTCPConnection ne fonctionne pas
        try {
            $netstatOutput = netstat -ano | Where-Object { $_ -match ":$Port\s" -and $_ -match "LISTENING" }
            if ($netstatOutput) {
                $pidMatch = $netstatOutput[0] -match "\s+(\d+)$"
                if ($pidMatch) {
                    $processId = $matches[1]
                    $proc = Get-Process -Id $processId -ErrorAction SilentlyContinue
                    if ($proc) {
                        return @{
                            Process = $proc
                            Connection = "Found via netstat"
                            Found = $true
                        }
                    }
                }
            }
        }
        catch {
            # Dernière tentative
        }
    }
    
    return @{ Found = $false }
}

function Test-ServiceHealth {
    param([string]$Url, [int]$TimeoutSec = 5)
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec $TimeoutSec -UseBasicParsing
        return @{
            Success = $true
            StatusCode = $response.StatusCode
            Content = $response.Content
        }
    }
    catch {
        return @{
            Success = $false
            Error = $_.Exception.Message
            StatusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode } else { "N/A" }
        }
    }
}

function Get-ServiceStatus {
    param([string]$ServiceKey, [hashtable]$Config)
    
    $status = @{
        Name = $Config.Name
        Port = $Config.Port
        PortOpen = $false
        ProcessFound = $false
        ProcessInfo = $null
        HealthCheck = $null
        SwaggerCheck = $null
    }
    
    # Test du port
    $status.PortOpen = Test-ServicePort -Port $Config.Port
    
    # Recherche du processus
    $processInfo = Get-ServiceProcess -ProcessNames $Config.ProcessName -Port $Config.Port
    if ($processInfo.Found) {
        $status.ProcessFound = $true
        $status.ProcessInfo = @{
            Name = $processInfo.Process.ProcessName
            Id = $processInfo.Process.Id
            StartTime = $processInfo.Process.StartTime
            WorkingSet = [math]::Round($processInfo.Process.WorkingSet64 / 1MB, 2)
        }
    }
    
    # Test health si le port est ouvert
    if ($status.PortOpen) {
        $healthUrl = "http://localhost:$($Config.Port)$($Config.HealthEndpoint)"
        $status.HealthCheck = Test-ServiceHealth -Url $healthUrl
        
        $swaggerUrl = "http://localhost:$($Config.Port)$($Config.SwaggerEndpoint)"
        $status.SwaggerCheck = Test-ServiceHealth -Url $swaggerUrl
    }
    
    return $status
}

function Show-ServiceStatus {
    param([hashtable]$Status, [bool]$Detailed = $false)
    
    $statusIcon = if ($Status.PortOpen -and $Status.ProcessFound) { "🟢" } 
                  elseif ($Status.PortOpen) { "🟡" } 
                  else { "🔴" }
    
    Write-Host "$statusIcon $($Status.Name)" -ForegroundColor White
    Write-Host "   Port $($Status.Port): $(if ($Status.PortOpen) { 'OUVERT' } else { 'FERME' })" -ForegroundColor $(if ($Status.PortOpen) { 'Green' } else { 'Red' })
    
    if ($Status.ProcessFound -and $Status.ProcessInfo) {
        Write-Host "   Processus: $($Status.ProcessInfo.Name) (PID: $($Status.ProcessInfo.Id))" -ForegroundColor Green
        if ($Detailed) {
            Write-Host "   Mémoire: $($Status.ProcessInfo.WorkingSet) MB" -ForegroundColor Gray
            Write-Host "   Démarré: $($Status.ProcessInfo.StartTime)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   Processus: NON TROUVE" -ForegroundColor Red
    }
    
    if ($Status.HealthCheck) {
        $healthStatus = if ($Status.HealthCheck.Success) { "OK" } else { "ERREUR" }
        $healthColor = if ($Status.HealthCheck.Success) { "Green" } else { "Red" }
        Write-Host "   Health: $healthStatus" -ForegroundColor $healthColor
        
        if ($Detailed -and -not $Status.HealthCheck.Success) {
            Write-Host "   Erreur: $($Status.HealthCheck.Error)" -ForegroundColor Red
        }
    }
    
    if ($Status.SwaggerCheck -and $Detailed) {
        $swaggerStatus = if ($Status.SwaggerCheck.Success) { "OK" } else { "ERREUR" }
        $swaggerColor = if ($Status.SwaggerCheck.Success) { "Green" } else { "Red" }
        Write-Host "   Swagger: $swaggerStatus" -ForegroundColor $swaggerColor
    }
    
    Write-Host ""
}

# Exécution principale
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "VERIFICATION DES SERVICES - NIESPRO" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Heure: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

$results = @{}

if ($Service -eq "all") {
    foreach ($serviceKey in $serviceConfig.Keys) {
        $status = Get-ServiceStatus -ServiceKey $serviceKey -Config $serviceConfig[$serviceKey]
        $results[$serviceKey] = $status
        Show-ServiceStatus -Status $status -Detailed $Detailed
    }
} else {
    if ($serviceConfig.ContainsKey($Service.ToLower())) {
        $status = Get-ServiceStatus -ServiceKey $Service -Config $serviceConfig[$Service.ToLower()]
        $results[$Service] = $status
        Show-ServiceStatus -Status $status -Detailed $Detailed
    } else {
        Write-Host "Service '$Service' non reconnu" -ForegroundColor Red
        Write-Host "Services disponibles: $($serviceConfig.Keys -join ', ')" -ForegroundColor Yellow
    }
}

# Résumé
$activeServices = ($results.Values | Where-Object { $_.PortOpen -and $_.ProcessFound }).Count
$totalServices = $results.Count

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "RESUME: $activeServices/$totalServices services actifs" -ForegroundColor $(if ($activeServices -eq $totalServices) { 'Green' } else { 'Yellow' })
Write-Host "===============================================" -ForegroundColor Cyan

if ($Json) {
    $results | ConvertTo-Json -Depth 3
}