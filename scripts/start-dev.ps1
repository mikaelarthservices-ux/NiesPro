# Script de démarrage pour l'environnement de développement NiesPro
param(
    [switch]$Clean = $false
)

Write-Host "🚀 Démarrage de l'environnement de développement NiesPro..." -ForegroundColor Green

# Vérification des prérequis
Write-Host "📋 Vérification des prérequis..." -ForegroundColor Yellow

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker n'est pas installé." -ForegroundColor Red
    exit 1
}

if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker Compose n'est pas installé." -ForegroundColor Red
    exit 1
}

# Arrêt des services existants
Write-Host "🛑 Arrêt des services existants..." -ForegroundColor Yellow
docker-compose down

# Nettoyage des conteneurs et volumes (optionnel)
if ($Clean) {
    Write-Host "🧹 Nettoyage complet..." -ForegroundColor Yellow
    docker-compose down -v
    docker system prune -f
}

# Compilation et démarrage des services
Write-Host "🔨 Compilation et démarrage des services..." -ForegroundColor Yellow
docker-compose up --build -d

# Attente que les services soient prêts
Write-Host "⏳ Attente que les services soient prêts..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Vérification des services
Write-Host "🔍 Vérification des services..." -ForegroundColor Yellow

$services = @(
    @{Name="MySQL"; Port=3306},
    @{Name="Redis"; Port=6379},
    @{Name="Auth API"; Port=8001},
    @{Name="RabbitMQ"; Port=15672},
    @{Name="Elasticsearch"; Port=9200},
    @{Name="Kibana"; Port=5601},
    @{Name="Seq"; Port=5341}
)

foreach ($service in $services) {
    try {
        $connection = Test-NetConnection -ComputerName localhost -Port $service.Port -WarningAction SilentlyContinue
        if ($connection.TcpTestSucceeded) {
            Write-Host "✅ $($service.Name) (port $($service.Port)) - OK" -ForegroundColor Green
        } else {
            Write-Host "❌ $($service.Name) (port $($service.Port)) - ERREUR" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "❌ $($service.Name) (port $($service.Port)) - ERREUR" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎉 Environnement NiesPro démarré avec succès!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Services disponibles :" -ForegroundColor Cyan
Write-Host "   🔐 Auth API:        http://localhost:8001" -ForegroundColor White
Write-Host "   🔐 Auth API Health: http://localhost:8001/api/health" -ForegroundColor White
Write-Host "   🔐 Auth API Swagger: http://localhost:8001" -ForegroundColor White
Write-Host "   🗄️  MySQL:          localhost:3306" -ForegroundColor White
Write-Host "   🔴 Redis:           localhost:6379" -ForegroundColor White
Write-Host "   🐰 RabbitMQ:        http://localhost:15672 (niespro/NiesPro2025!Rabbit)" -ForegroundColor White
Write-Host "   📊 Elasticsearch:   http://localhost:9200" -ForegroundColor White
Write-Host "   📈 Kibana:          http://localhost:5601" -ForegroundColor White
Write-Host "   📝 Seq:             http://localhost:5341" -ForegroundColor White
Write-Host ""
Write-Host "Pour arrêter l'environnement: docker-compose down" -ForegroundColor Yellow
Write-Host "Pour voir les logs: docker-compose logs -f [service-name]" -ForegroundColor Yellow