# =======================================================================
# NIESPRO DATABASE STATUS CHECKER
# Vérifie l'état des services de base de données
# =======================================================================

Write-Host "🔍 NIESPRO DATABASE STATUS CHECKER" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green

# Test de connectivité MySQL (port 3306)
Write-Host "`n🔌 Test de connectivité MySQL..." -ForegroundColor Yellow
try {
    $mysqlTest = Test-NetConnection -ComputerName localhost -Port 3306 -WarningAction SilentlyContinue
    if ($mysqlTest.TcpTestSucceeded) {
        Write-Host "✅ MySQL est accessible sur le port 3306" -ForegroundColor Green
        
        # Essayer de trouver les outils MySQL/WAMP
        $wampPaths = @(
            "C:\wamp64\www",
            "C:\wamp\www", 
            "C:\xampp\htdocs"
        )
        
        $foundWamp = $false
        foreach ($path in $wampPaths) {
            if (Test-Path $path) {
                Write-Host "✅ Serveur web trouvé: $path" -ForegroundColor Green
                Write-Host "🌐 phpMyAdmin probablement accessible: http://localhost/phpmyadmin/" -ForegroundColor Cyan
                $foundWamp = $true
                break
            }
        }
        
        if (-not $foundWamp) {
            Write-Host "⚠️  Serveur web non détecté dans les emplacements standards" -ForegroundColor Yellow
        }
        
    } else {
        Write-Host "❌ MySQL n'est pas accessible sur le port 3306" -ForegroundColor Red
        Write-Host "💡 MySQL/MariaDB n'est peut-être pas démarré" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Erreur lors du test de connectivité: $($_.Exception.Message)" -ForegroundColor Red
}

# Vérifier les migrations Catalog
Write-Host "`n📦 Vérification du projet Catalog..." -ForegroundColor Yellow
$catalogPath = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Catalog"
if (Test-Path "$catalogPath\Catalog.API") {
    Write-Host "✅ Projet Catalog.API trouvé" -ForegroundColor Green
    
    # Vérifier les migrations
    if (Test-Path "$catalogPath\Catalog.Infrastructure\Migrations") {
        $migrations = Get-ChildItem "$catalogPath\Catalog.Infrastructure\Migrations" -Filter "*.cs"
        Write-Host "✅ $($migrations.Count) migrations trouvées" -ForegroundColor Green
    } else {
        Write-Host "❌ Dossier Migrations introuvable" -ForegroundColor Red
    }
    
    # Vérifier la configuration
    $configPath = "$catalogPath\Catalog.API\appsettings.Development.json"
    if (Test-Path $configPath) {
        Write-Host "✅ Configuration de développement trouvée" -ForegroundColor Green
        
        # Lire la chaîne de connexion
        try {
            $config = Get-Content $configPath | ConvertFrom-Json
            $connectionString = $config.ConnectionStrings.DefaultConnection
            if ($connectionString -like "*niespro_catalog_dev*") {
                Write-Host "✅ Chaîne de connexion configurée pour niespro_catalog_dev" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠️  Impossible de lire la configuration" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "❌ Projet Catalog.API introuvable" -ForegroundColor Red
}

# État du service Catalog.API
Write-Host "`n🚀 État du service Catalog.API..." -ForegroundColor Yellow
$catalogProcess = Get-Process | Where-Object {$_.ProcessName -like "*Catalog*"}
if ($catalogProcess) {
    Write-Host "✅ Service Catalog.API en cours d'exécution (PID: $($catalogProcess.Id))" -ForegroundColor Green
    
    # Tester les endpoints
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5003/health" -UseBasicParsing -TimeoutSec 5
        Write-Host "✅ Endpoint health accessible (Status: $($response.StatusCode))" -ForegroundColor Green
    } catch {
        Write-Host "⚠️  Endpoint health non accessible" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Service Catalog.API non démarré" -ForegroundColor Red
}

# Résumé et actions recommandées
Write-Host "`n📋 RÉSUMÉ ET ACTIONS:" -ForegroundColor Magenta
Write-Host "===================" -ForegroundColor Magenta

Write-Host "`n🎯 ÉTAPES RECOMMANDÉES:" -ForegroundColor Yellow

if ($mysqlTest.TcpTestSucceeded) {
    Write-Host "1. ✅ MySQL est accessible" -ForegroundColor Green
    Write-Host "2. 🔧 Appliquer les migrations:" -ForegroundColor Yellow
    Write-Host "   cd src\Services\Catalog\Catalog.API" -ForegroundColor Gray
    Write-Host "   dotnet ef database update --project ..\Catalog.Infrastructure" -ForegroundColor Gray
    Write-Host "3. 🚀 Démarrer le service:" -ForegroundColor Yellow
    Write-Host "   dotnet run" -ForegroundColor Gray
} else {
    Write-Host "1. ❌ Démarrer MySQL/WAMP:" -ForegroundColor Red
    Write-Host "   - Installer WAMP, XAMPP ou MySQL Server" -ForegroundColor Gray
    Write-Host "   - Démarrer les services MySQL" -ForegroundColor Gray
    Write-Host "2. 🔧 Puis appliquer les migrations" -ForegroundColor Yellow
    Write-Host "3. 🚀 Puis démarrer le service" -ForegroundColor Yellow
}

Write-Host "`n🛠️  OUTILS DISPONIBLES:" -ForegroundColor Yellow
Write-Host "- Setup DB: tools\catalog-db-setup.ps1" -ForegroundColor White
Write-Host "- Test MySQL: tools\check-mysql.bat" -ForegroundColor White
Write-Host "- Inspector: tools\mysql-inspector.ps1" -ForegroundColor White

Write-Host "`n🌐 LIENS UTILES:" -ForegroundColor Yellow
Write-Host "- phpMyAdmin: http://localhost/phpmyadmin/" -ForegroundColor Cyan
Write-Host "- Catalog API: http://localhost:5003" -ForegroundColor Cyan  
Write-Host "- Swagger: http://localhost:5003/swagger" -ForegroundColor Cyan

Write-Host "`n✅ Diagnostic terminé!" -ForegroundColor Green