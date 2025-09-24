# ===============================================
# Script de Diagnostic Rapide des Services
# ===============================================

Write-Host "=== DIAGNOSTIC SERVICES NIESPRO ===" -ForegroundColor Cyan
Write-Host "Heure: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Configuration des services
$services = @{
    "Auth.API" = @{ Port = 5001; Process = "Auth.API" }
    "Customer.API" = @{ Port = 5009; Process = "Customer.API" }
    "Catalog.API" = @{ Port = 5004; Process = "Catalog.API" }
    "Order.API" = @{ Port = 5003; Process = "Order.API" }
    "Payment.API" = @{ Port = 5002; Process = "Payment.API" }
    "Gateway.API" = @{ Port = 5000; Process = "Gateway.API" }
}

# Test des ports
Write-Host "🔍 PORTS EN ECOUTE:" -ForegroundColor Yellow
foreach ($serviceName in $services.Keys) {
    $port = $services[$serviceName].Port
    $listening = netstat -ano | findstr ":$port " | findstr "LISTENING"
    
    if ($listening) {
        $pid = ($listening -split '\s+')[-1]
        Write-Host "  ✅ $serviceName (Port $port) - PID: $pid" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $serviceName (Port $port) - NON ACTIF" -ForegroundColor Red
    }
}

Write-Host ""

# Test MySQL
Write-Host "🗄️  MYSQL:" -ForegroundColor Yellow
$mysqlProcess = Get-Process | Where-Object { $_.ProcessName -like "*mysql*" }
if ($mysqlProcess) {
    Write-Host "  ✅ MySQL actif (PID: $($mysqlProcess.Id -join ', '))" -ForegroundColor Green
} else {
    Write-Host "  ❌ MySQL non détecté" -ForegroundColor Red
}

Write-Host ""

# Test de connectivité des services actifs
Write-Host "🏥 HEALTH CHECKS:" -ForegroundColor Yellow
foreach ($serviceName in $services.Keys) {
    $port = $services[$serviceName].Port
    $listening = netstat -ano | findstr ":$port " | findstr "LISTENING"
    
    if ($listening) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$port/health" -Method GET -TimeoutSec 3 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-Host "  ✅ $serviceName Health: OK" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️  $serviceName Health: Status $($response.StatusCode)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  ❌ $serviceName Health: ERROR ($($_.Exception.Message.Split('.')[0]))" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "=== FIN DU DIAGNOSTIC ===" -ForegroundColor Cyan