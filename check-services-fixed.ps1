# Script PowerShell pour verifier l'etat des services NiesPro
# Compatible Windows avec detection robuste des ports

param(
    [switch]$Detailed
)

# Configuration des services
$services = @(
    @{ Name = "Gateway API"; Port = 5000; Protocol = "https" },
    @{ Name = "Auth API"; Port = 5001; Protocol = "https" },
    @{ Name = "Order API"; Port = 5002; Protocol = "https" },
    @{ Name = "Catalog API"; Port = 5003; Protocol = "https" },
    @{ Name = "Payment API"; Port = 5004; Protocol = "https" }
)

Write-Host "=== Verification des Services NiesPro ===" -ForegroundColor Cyan
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host

function Test-PortListening {
    param(
        [int]$Port
    )
    
    try {
        # Methode 1: Test avec TcpClient (plus fiable que netstat)
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $connection = $tcpClient.BeginConnect("localhost", $Port, $null, $null)
        $wait = $connection.AsyncWaitHandle.WaitOne(1000, $false)
        
        if ($wait) {
            $tcpClient.EndConnect($connection)
            $tcpClient.Close()
            return $true
        } else {
            $tcpClient.Close()
            return $false
        }
    } catch {
        return $false
    }
}

function Get-ProcessOnPort {
    param(
        [int]$Port
    )
    
    try {
        # Utiliser Get-NetTCPConnection (plus moderne que netstat)
        $connection = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
        if ($connection) {
            $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
            return $process.ProcessName
        }
    } catch {
        # Fallback avec netstat si Get-NetTCPConnection echoue
        try {
            $netstatOutput = netstat -ano | Where-Object { $_ -match ":$Port\s" -and $_ -match "LISTENING" }
            if ($netstatOutput) {
                $pid = ($netstatOutput -split '\s+')[-1]
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                return $process.ProcessName
            }
        } catch {
            return "Inconnu"
        }
    }
    return $null
}

# Verification de chaque service
$results = @()
foreach ($service in $services) {
    $isListening = Test-PortListening -Port $service.Port
    $processName = if ($isListening) { Get-ProcessOnPort -Port $service.Port } else { $null }
    
    $status = if ($isListening) { "[ONLINE]" } else { "[OFFLINE]" }
    $url = "$($service.Protocol)://localhost:$($service.Port)"
    
    $result = [PSCustomObject]@{
        Service = $service.Name
        Port = $service.Port
        Status = $status
        IsListening = $isListening
        Process = $processName
        URL = $url
    }
    
    $results += $result
    
    # Affichage immediat
    $color = if ($isListening) { "Green" } else { "Red" }
    Write-Host "[$($service.Port)] " -NoNewline -ForegroundColor Yellow
    Write-Host "$($service.Name): " -NoNewline -ForegroundColor White
    Write-Host $status -ForegroundColor $color
    
    if ($Detailed -and $processName) {
        Write-Host "    Processus: $processName" -ForegroundColor Gray
        Write-Host "    URL: $url" -ForegroundColor Gray
    }
}

Write-Host
Write-Host "=== Resume ===" -ForegroundColor Cyan
$onlineCount = ($results | Where-Object { $_.IsListening }).Count
$totalCount = $results.Count

Write-Host "Services en ligne: $onlineCount/$totalCount" -ForegroundColor $(if ($onlineCount -eq $totalCount) { "Green" } else { "Yellow" })

if ($onlineCount -lt $totalCount) {
    Write-Host
    Write-Host "Services hors ligne:" -ForegroundColor Red
    $results | Where-Object { -not $_.IsListening } | ForEach-Object {
        Write-Host "  - $($_.Service) (Port $($_.Port))" -ForegroundColor Red
    }
}

# Retourner les resultats pour utilisation programmatique
return $results