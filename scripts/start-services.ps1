# Script PowerShell pour lancer l'architecture NiesPro
param(
    [switch]$StopAll,
    [switch]$AuthOnly,
    [switch]$OrderOnly,
    [switch]$CatalogOnly,
    [switch]$GatewayOnly
)

function Write-Banner {
    param($Title, $Color = "Cyan")
    Write-Host "========================================" -ForegroundColor $Color
    Write-Host "   $Title" -ForegroundColor $Color
    Write-Host "========================================" -ForegroundColor $Color
    Write-Host ""
}

function Start-Service {
    param($Name, $Port, $ProjectPath)
    
    Write-Host "Démarrage de $Name sur le port $Port..." -ForegroundColor Green
    
    $scriptBlock = {
        param($Name, $Port, $ProjectPath)
        $Host.UI.RawUI.WindowTitle = "NiesPro $($Name) (Port $($Port))"
        Set-Location $ProjectPath
        Write-Host "Building $Name..." -ForegroundColor Yellow
        dotnet build
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed!" -ForegroundColor Red
            Read-Host "Press Enter to exit"
            return
        }
        Write-Host "Starting $Name on https://localhost:$Port..." -ForegroundColor Green
        dotnet run --urls "https://localhost:$Port"
        Read-Host "Press Enter to exit"
    }
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "& {$($scriptBlock.ToString())} '$Name' '$Port' '$ProjectPath'"
    Start-Sleep 2
}

function Stop-AllServices {
    Write-Banner "Arrêt de tous les services NiesPro" "Red"
    
    Write-Host "Arrêt de tous les processus dotnet..." -ForegroundColor Yellow
    Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
    
    Write-Host "Vérification des ports..." -ForegroundColor Yellow
    $ports = @(5000, 5001, 5002, 5003)
    $activeConnections = netstat -an | Select-String "LISTENING" | Select-String ($ports -join "|")
    
    if ($activeConnections) {
        Write-Host "Certains ports sont encore utilisés :" -ForegroundColor Yellow
        $activeConnections | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    } else {
        Write-Host "Tous les services ont été arrêtés avec succès." -ForegroundColor Green
    }
}

$basePath = "C:\Users\HP\Documents\projets\NiesPro\src"

if ($StopAll) {
    Stop-AllServices
    return
}

Write-Banner "Architecture Microservices NiesPro"

if ($AuthOnly) {
    Start-Service "Auth.API" 5001 "$basePath\Services\Auth\Auth.API"
} elseif ($OrderOnly) {
    Start-Service "Order.API" 5002 "$basePath\Services\Order\Order.API"
} elseif ($CatalogOnly) {
    Start-Service "Catalog.API" 5003 "$basePath\Services\Catalog\Catalog.API"
} elseif ($GatewayOnly) {
    Start-Service "Gateway.API" 5000 "$basePath\Gateway\Gateway.API"
} else {
    # Lancement de tous les services
    Write-Host "Lancement de tous les microservices..." -ForegroundColor Cyan
    Write-Host ""
    
    Start-Service "Auth.API" 5001 "$basePath\Services\Auth\Auth.API"
    Start-Service "Order.API" 5002 "$basePath\Services\Order\Order.API"
    Start-Service "Catalog.API" 5003 "$basePath\Services\Catalog\Catalog.API"
    Start-Service "Gateway.API" 5000 "$basePath\Gateway\Gateway.API"
    
    Write-Banner "Tous les services démarrés" "Green"
    Write-Host "URLs d'accès :" -ForegroundColor White
    Write-Host "- Gateway API: https://localhost:5000" -ForegroundColor Yellow
    Write-Host "- Auth API: https://localhost:5001" -ForegroundColor Yellow
    Write-Host "- Order API: https://localhost:5002" -ForegroundColor Yellow
    Write-Host "- Catalog API: https://localhost:5003" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Documentation Swagger disponible sur chaque service via /swagger" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Utilisez 'powershell -File start-services.ps1 -StopAll' pour arrêter tous les services" -ForegroundColor Gray