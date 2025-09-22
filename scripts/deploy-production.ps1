# ===============================================
# SCRIPT DE DÉPLOIEMENT PRODUCTION NIESPRO ERP
# ===============================================

Write-Host "🚀 DÉPLOIEMENT PRODUCTION NIESPRO ERP" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

# Vérification des prérequis
Write-Host ""
Write-Host "📋 VÉRIFICATION DES PRÉREQUIS" -ForegroundColor Yellow

# Docker
try {
    $dockerVersion = docker --version
    Write-Host "✅ Docker: $dockerVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ Docker n'est pas installé ou accessible" -ForegroundColor Red
    exit 1
}

# Docker Compose
try {
    $dockerComposeVersion = docker compose version
    Write-Host "✅ Docker Compose: $dockerComposeVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ Docker Compose n'est pas installé ou accessible" -ForegroundColor Red
    exit 1
}

# Vérification du fichier .env
if (Test-Path ".env.production") {
    Write-Host "✅ Fichier de configuration .env.production trouvé" -ForegroundColor Green
}
else {
    Write-Host "⚠️  Fichier .env.production non trouvé. Utilisation des valeurs par défaut." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🔧 PRÉPARATION DE L'ENVIRONNEMENT" -ForegroundColor Yellow

# Création des répertoires de logs
$logDirs = @("logs/auth", "logs/catalog", "logs/order", "logs/payment", "logs/stock", "logs/gateway")
foreach ($dir in $logDirs) {
    if (!(Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force
        Write-Host "📁 Répertoire créé: $dir" -ForegroundColor Cyan
    }
}

# Génération des certificats SSL pour le développement
Write-Host ""
Write-Host "🔐 CONFIGURATION SSL" -ForegroundColor Yellow
try {
    dotnet dev-certs https --clean
    dotnet dev-certs https --trust
    Write-Host "✅ Certificats SSL configurés" -ForegroundColor Green
}
catch {
    Write-Host "⚠️  Problème avec les certificats SSL" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🏗️  CONSTRUCTION DES IMAGES DOCKER" -ForegroundColor Yellow

# Arrêt des services existants
Write-Host "🛑 Arrêt des services existants..."
docker compose -f docker-compose.production.yml down

# Construction des images
Write-Host "🔨 Construction des images Docker..."
docker compose -f docker-compose.production.yml build --no-cache

Write-Host ""
Write-Host "🚀 DÉMARRAGE DES SERVICES" -ForegroundColor Yellow

# Démarrage de l'infrastructure (base de données)
Write-Host "📊 Démarrage de l'infrastructure..."
docker compose -f docker-compose.production.yml up -d mysql redis

# Attente de la disponibilité de MySQL
Write-Host "⏳ Attente de la disponibilité de MySQL..."
$maxAttempts = 30
$attempt = 0
do {
    Start-Sleep -Seconds 10
    $attempt++
    $mysqlReady = docker compose -f docker-compose.production.yml exec mysql mysqladmin ping -h localhost --silent
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL est prêt" -ForegroundColor Green
        break
    }
    Write-Host "⏳ Tentative $attempt/$maxAttempts..." -ForegroundColor Yellow
} while ($attempt -lt $maxAttempts)

if ($attempt -eq $maxAttempts) {
    Write-Host "❌ MySQL n'est pas disponible après $maxAttempts tentatives" -ForegroundColor Red
    exit 1
}

# Démarrage des microservices
Write-Host "🔄 Démarrage des microservices..."
docker compose -f docker-compose.production.yml up -d stock-api auth-api catalog-api order-api payment-api

# Attente que les services soient prêts
Write-Host "⏳ Attente de la disponibilité des microservices..."
Start-Sleep -Seconds 30

# Démarrage du gateway
Write-Host "🌐 Démarrage de l'API Gateway..."
docker compose -f docker-compose.production.yml up -d gateway-api

# Démarrage du monitoring
Write-Host "📊 Démarrage du monitoring..."
docker compose -f docker-compose.production.yml up -d prometheus grafana elasticsearch kibana

Write-Host ""
Write-Host "🔍 VÉRIFICATION DES SERVICES" -ForegroundColor Yellow

# Test des endpoints de santé
$services = @(
    @{ Name = "Stock API"; Url = "http://localhost:5005/health" },
    @{ Name = "Auth API"; Url = "http://localhost:5001/health" },
    @{ Name = "Catalog API"; Url = "http://localhost:5003/health" },
    @{ Name = "Order API"; Url = "http://localhost:5002/health" },
    @{ Name = "Payment API"; Url = "http://localhost:5004/health" },
    @{ Name = "Gateway"; Url = "http://localhost:5000/health" }
)

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($service.Name): Opérationnel" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️  $($service.Name): Status $($response.StatusCode)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "❌ $($service.Name): Non accessible" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "📋 RÉSUMÉ DU DÉPLOIEMENT" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 URLs des services:"
Write-Host "  • Gateway HTTP:     http://localhost:5000" -ForegroundColor Cyan
Write-Host "  • Gateway HTTPS:    https://localhost:5010" -ForegroundColor Cyan
Write-Host "  • Auth API:         http://localhost:5001" -ForegroundColor Cyan
Write-Host "  • Catalog API:      http://localhost:5003" -ForegroundColor Cyan
Write-Host "  • Order API:        http://localhost:5002" -ForegroundColor Cyan
Write-Host "  • Payment API:      http://localhost:5004" -ForegroundColor Cyan
Write-Host "  • Stock API:        http://localhost:5005" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Monitoring:"
Write-Host "  • Prometheus:       http://localhost:9090" -ForegroundColor Cyan
Write-Host "  • Grafana:          http://localhost:3000 (admin/NiesPro2024!)" -ForegroundColor Cyan
Write-Host "  • Kibana:           http://localhost:5601" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Base de données:"
Write-Host "  • MySQL:            localhost:3306 (root/NiesPro2024!)" -ForegroundColor Cyan
Write-Host "  • Redis:            localhost:6379" -ForegroundColor Cyan
Write-Host ""
Write-Host "🎯 Déploiement terminé avec succès!" -ForegroundColor Green
Write-Host ""
Write-Host "🔧 Commandes utiles:"
Write-Host "  • Logs:             docker compose -f docker-compose.production.yml logs -f [service]" -ForegroundColor Gray
Write-Host "  • Status:           docker compose -f docker-compose.production.yml ps" -ForegroundColor Gray
Write-Host "  • Arrêt:            docker compose -f docker-compose.production.yml down" -ForegroundColor Gray