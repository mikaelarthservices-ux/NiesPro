# Script PowerShell pour démarrer et vérifier la base de données NiesPro

Write-Host "🚀 Démarrage de la base de données NiesPro..." -ForegroundColor Green

# Vérifier si Docker est en cours d'exécution
try {
    $dockerStatus = docker version 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Docker n'est pas démarré. Veuillez démarrer Docker Desktop." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Docker est actif" -ForegroundColor Green
}
catch {
    Write-Host "❌ Docker n'est pas installé ou accessible." -ForegroundColor Red
    exit 1
}

# Démarrer MySQL et Redis
Write-Host "🗄️ Démarrage de MySQL et Redis..." -ForegroundColor Yellow
docker-compose up -d mysql redis

# Attendre que MySQL soit prêt
Write-Host "⏳ Attente que MySQL soit prêt..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0

do {
    Start-Sleep -Seconds 2
    $attempt++
    
    try {
        $result = docker exec niespro-mysql mysqladmin ping -u root -pNiesPro2025!Root 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ MySQL est prêt!" -ForegroundColor Green
            break
        }
    }
    catch {
        # Continuer à attendre
    }
    
    if ($attempt -ge $maxAttempts) {
        Write-Host "❌ MySQL n'a pas démarré dans les temps." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "   Tentative $attempt/$maxAttempts..." -ForegroundColor Gray
} while ($true)

# Vérifier les bases de données
Write-Host "🔍 Vérification des bases de données..." -ForegroundColor Yellow
$databases = docker exec niespro-mysql mysql -u root -pNiesPro2025!Root -e "SHOW DATABASES;" 2>$null

if ($databases -match "NiesPro_Auth") {
    Write-Host "✅ Base de données NiesPro_Auth trouvée" -ForegroundColor Green
} else {
    Write-Host "⚠️ Base de données NiesPro_Auth non trouvée - sera créée par les migrations" -ForegroundColor Yellow
}

# Afficher les informations de connexion
Write-Host ""
Write-Host "🎉 Base de données prête!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Informations de connexion:" -ForegroundColor Cyan
Write-Host "   🖥️  Host: localhost" -ForegroundColor White
Write-Host "   📡 Port: 3306" -ForegroundColor White
Write-Host "   👤 User: root" -ForegroundColor White
Write-Host "   🔑 Password: NiesPro2025!Root" -ForegroundColor White
Write-Host "   🗄️  Database: NiesPro_Auth" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Interfaces disponibles:" -ForegroundColor Cyan
Write-Host "   🔧 Adminer (Web): http://localhost:8080" -ForegroundColor White
Write-Host "   📊 Redis: localhost:6379" -ForegroundColor White
Write-Host ""

# Démarrer Adminer pour l'interface web
Write-Host "🌐 Démarrage d'Adminer (interface web)..." -ForegroundColor Yellow
docker run -d --name niespro-adminer --network niespro-network -p 8080:8080 adminer 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Adminer démarré sur http://localhost:8080" -ForegroundColor Green
} else {
    # Adminer pourrait déjà être démarré
    Write-Host "⚠️ Adminer pourrait déjà être en cours d'exécution" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎯 Prochaines étapes:" -ForegroundColor Cyan
Write-Host "   1. Ouvrir http://localhost:8080" -ForegroundColor White
Write-Host "   2. Se connecter avec les infos ci-dessus" -ForegroundColor White
Write-Host "   3. Explorer la structure de la base" -ForegroundColor White
Write-Host ""

# Afficher les conteneurs en cours
Write-Host "📦 Conteneurs actifs:" -ForegroundColor Cyan
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"