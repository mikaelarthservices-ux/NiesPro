# Script d'arrêt de tous les services NiesPro
Write-Host "🛑 ARRÊT DE TOUS LES SERVICES NIESPRO" -ForegroundColor Red
Write-Host "====================================" -ForegroundColor Red

# Ports des services NiesPro
$niesproPorts = @(5000, 5001, 5002, 5003, 5004, 5005, 5006, 5010, 5011, 5012, 5013, 5014, 7001, 7011)

Write-Host ""
Write-Host "🔍 Vérification des services en cours..." -ForegroundColor Yellow

$runningServices = @()

foreach ($port in $niesproPorts) {
    $process = netstat -ano | findstr ":$port" | findstr "LISTENING"
    if ($process) {
        $pid = ($process -split '\s+')[-1]
        $runningServices += @{
            Port = $port
            PID = $pid
            Process = $process
        }
        Write-Host "  📍 Port $port : PID $pid" -ForegroundColor Cyan
    }
}

if ($runningServices.Count -eq 0) {
    Write-Host "✅ Aucun service NiesPro en cours d'exécution" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 ÉTAT DES PORTS NIESPRO:" -ForegroundColor Yellow
    Write-Host "  • Port 5001 (Auth.API)      : 🔴 Arrêté" -ForegroundColor White
    Write-Host "  • Port 5002 (Order.API)     : 🔴 Arrêté" -ForegroundColor White
    Write-Host "  • Port 5003 (Catalog.API)   : 🔴 Arrêté" -ForegroundColor White
    Write-Host "  • Port 5004 (Payment.API)   : 🔴 Arrêté" -ForegroundColor White
    Write-Host "  • Port 5005 (Stock.API)     : 🔴 Arrêté" -ForegroundColor White
    Write-Host "  • Port 5006 (Stock.API HTTPS): 🔴 Arrêté" -ForegroundColor White
    Write-Host "  • Port 7001 (Restaurant.API): 🔴 Arrêté" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "⚠️  $($runningServices.Count) service(s) encore en cours. Arrêt en cours..." -ForegroundColor Yellow
    
    foreach ($service in $runningServices) {
        Write-Host "🛑 Arrêt du processus PID $($service.PID) (Port $($service.Port))..." -ForegroundColor Red
        try {
            taskkill /PID $service.PID /F
            Write-Host "  ✅ Processus $($service.PID) arrêté" -ForegroundColor Green
        }
        catch {
            Write-Host "  ❌ Échec de l'arrêt du processus $($service.PID)" -ForegroundColor Red
        }
    }
    
    # Vérification finale
    Start-Sleep -Seconds 2
    Write-Host ""
    Write-Host "🔍 Vérification finale..." -ForegroundColor Yellow
    $stillRunning = 0
    foreach ($port in $niesproPorts) {
        $process = netstat -ano | findstr ":$port" | findstr "LISTENING"
        if ($process) {
            $stillRunning++
            Write-Host "  ⚠️  Port $port encore actif" -ForegroundColor Yellow
        }
    }
    
    if ($stillRunning -eq 0) {
        Write-Host "✅ Tous les services NiesPro sont maintenant arrêtés" -ForegroundColor Green
    } else {
        Write-Host "❌ $stillRunning service(s) encore actif(s)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎯 Services NiesPro arrêtés avec succès!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Commandes utiles:" -ForegroundColor White
Write-Host "  • Redémarrer Stock.API : cd src/Services/Stock/Stock.API && dotnet run" -ForegroundColor Gray
Write-Host "  • Vérifier les ports    : netstat -ano | findstr ':500'" -ForegroundColor Gray
Write-Host "  • Démarrer tous        : ./scripts/start-services.ps1" -ForegroundColor Gray