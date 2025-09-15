#!/bin/bash

echo "🚀 Démarrage de l'environnement de développement NiesPro..."

# Vérification des prérequis
echo "📋 Vérification des prérequis..."
command -v docker >/dev/null 2>&1 || { echo "❌ Docker n'est pas installé."; exit 1; }
command -v docker-compose >/dev/null 2>&1 || { echo "❌ Docker Compose n'est pas installé."; exit 1; }

# Arrêt des services existants
echo "🛑 Arrêt des services existants..."
docker-compose down

# Nettoyage des conteneurs et volumes (optionnel)
if [ "$1" == "--clean" ]; then
    echo "🧹 Nettoyage complet..."
    docker-compose down -v
    docker system prune -f
fi

# Compilation et démarrage des services
echo "🔨 Compilation et démarrage des services..."
docker-compose up --build -d

# Attente que les services soient prêts
echo "⏳ Attente que les services soient prêts..."
sleep 30

# Vérification des services
echo "🔍 Vérification des services..."
services=(
    "mysql:3306"
    "redis:6379"
    "auth-api:8001"
)

for service in "${services[@]}"; do
    IFS=':' read -r name port <<< "$service"
    if nc -z localhost "$port" 2>/dev/null; then
        echo "✅ $name (port $port) - OK"
    else
        echo "❌ $name (port $port) - ERREUR"
    fi
done

echo ""
echo "🎉 Environnement NiesPro démarré avec succès!"
echo ""
echo "📋 Services disponibles :"
echo "   🔐 Auth API:        http://localhost:8001"
echo "   🔐 Auth API Health: http://localhost:8001/api/health"
echo "   🔐 Auth API Swagger: http://localhost:8001"
echo "   🗄️  MySQL:          localhost:3306"
echo "   🔴 Redis:           localhost:6379"
echo "   🐰 RabbitMQ:        http://localhost:15672 (admin/admin)"
echo "   📊 Elasticsearch:   http://localhost:9200"
echo "   📈 Kibana:          http://localhost:5601"
echo "   📝 Seq:             http://localhost:5341"
echo ""
echo "Pour arrêter l'environnement: docker-compose down"
echo "Pour voir les logs: docker-compose logs -f [service-name]"