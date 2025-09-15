# Script de test pour WAMP et création des bases NiesPro

Write-Host "🚀 Test de configuration WAMP pour NiesPro" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Green

# Vérifier si WAMP est démarré (port 3306 MySQL)
Write-Host "📡 Vérification de WAMP MySQL (port 3306)..." -ForegroundColor Yellow

try {
    $mysqlTest = Test-NetConnection -ComputerName localhost -Port 3306 -WarningAction SilentlyContinue
    if ($mysqlTest.TcpTestSucceeded) {
        Write-Host "✅ MySQL WAMP est accessible sur le port 3306" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL WAMP n'est pas démarré" -ForegroundColor Red
        Write-Host "💡 Démarrez WAMP et assurez-vous que l'icône soit verte" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "❌ Erreur lors du test MySQL: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Vérifier si Apache est démarré (port 80)
Write-Host "🌐 Vérification d'Apache WAMP (port 80)..." -ForegroundColor Yellow

try {
    $apacheTest = Test-NetConnection -ComputerName localhost -Port 80 -WarningAction SilentlyContinue
    if ($apacheTest.TcpTestSucceeded) {
        Write-Host "✅ Apache WAMP est accessible sur le port 80" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Apache WAMP n'est pas démarré (ce n'est pas bloquant)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️ Impossible de tester Apache" -ForegroundColor Yellow
}

# Tester phpMyAdmin
Write-Host "📊 Test d'accès à phpMyAdmin..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost/phpmyadmin/" -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ phpMyAdmin est accessible" -ForegroundColor Green
    } else {
        Write-Host "⚠️ phpMyAdmin pourrait ne pas être accessible" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️ Impossible d'accéder à phpMyAdmin - vérifiez Apache" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎯 Instructions pour créer les bases NiesPro:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1️⃣ Ouvrir phpMyAdmin:" -ForegroundColor White
Write-Host "   👉 http://localhost/phpmyadmin/" -ForegroundColor Gray
Write-Host ""
Write-Host "2️⃣ Se connecter:" -ForegroundColor White
Write-Host "   👤 Utilisateur: root" -ForegroundColor Gray
Write-Host "   🔑 Mot de passe: (laisser vide ou votre mot de passe WAMP)" -ForegroundColor Gray
Write-Host ""
Write-Host "3️⃣ Exécuter le script SQL:" -ForegroundColor White
Write-Host "   📁 Aller dans l'onglet 'SQL'" -ForegroundColor Gray
Write-Host "   📝 Copier le contenu de: scripts/database/create_databases_wamp.sql" -ForegroundColor Gray
Write-Host "   ▶️ Cliquer 'Exécuter'" -ForegroundColor Gray
Write-Host ""
Write-Host "4️⃣ Vérifier le résultat:" -ForegroundColor White
Write-Host "   📊 Vous devriez voir 8 nouvelles bases 'NiesPro_*'" -ForegroundColor Gray
Write-Host ""

# Afficher le chemin du script SQL
$scriptPath = Join-Path $PSScriptRoot "..\database\create_databases_wamp.sql"
if (Test-Path $scriptPath) {
    Write-Host "📄 Script SQL trouvé:" -ForegroundColor Green
    Write-Host "   $scriptPath" -ForegroundColor Gray
} else {
    Write-Host "📄 Script SQL à utiliser:" -ForegroundColor Yellow
    Write-Host "   scripts/database/create_databases_wamp.sql" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🔗 Liens utiles:" -ForegroundColor Cyan
Write-Host "   🌐 phpMyAdmin: http://localhost/phpmyadmin/" -ForegroundColor White
Write-Host "   🏠 WAMP Accueil: http://localhost/" -ForegroundColor White
Write-Host ""

if ($mysqlTest.TcpTestSucceeded) {
    Write-Host "✅ WAMP est prêt pour NiesPro!" -ForegroundColor Green
} else {
    Write-Host "❌ Veuillez démarrer WAMP avant de continuer" -ForegroundColor Red
}