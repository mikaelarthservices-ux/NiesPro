# ===============================================
# GÉNÉRATEUR DE DOCUMENTATION API NIESPRO ERP
# ===============================================

Write-Host "📚 GÉNÉRATION DE LA DOCUMENTATION API" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Configuration des services opérationnels
$services = @(
    @{ 
        Name = "Auth.API"
        Port = 5001
        Description = "Service d'authentification et autorisation"
        BasePath = "src/Services/Auth/Auth.API"
        Endpoints = @(
            "/api/auth/login",
            "/api/auth/register", 
            "/api/auth/refresh",
            "/api/users",
            "/api/roles",
            "/health"
        )
    },
    @{ 
        Name = "Catalog.API"
        Port = 5003
        Description = "Gestion du catalogue produits"
        BasePath = "src/Services/Catalog/Catalog.API"
        Endpoints = @(
            "/api/products",
            "/api/categories",
            "/api/brands",
            "/api/inventory",
            "/health"
        )
    },
    @{ 
        Name = "Order.API"
        Port = 5002
        Description = "Gestion des commandes avec Event Sourcing"
        BasePath = "src/Services/Order/Order.API"
        Endpoints = @(
            "/api/orders",
            "/api/orders/{id}",
            "/api/orders/{id}/events",
            "/api/carts",
            "/health"
        )
    },
    @{ 
        Name = "Payment.API"
        Port = 5004
        Description = "Traitement des paiements multi-gateway"
        BasePath = "src/Services/Payment/Payment.API"
        Endpoints = @(
            "/api/payments",
            "/api/payments/{id}",
            "/api/payments/process",
            "/api/refunds",
            "/api/webhooks/stripe",
            "/api/webhooks/paypal",
            "/health"
        )
    },
    @{ 
        Name = "Stock.API"
        Port = 5005
        Description = "Gestion des stocks et inventaire"
        BasePath = "src/Services/Stock/Stock.API"
        Endpoints = @(
            "/api/stock",
            "/api/stock/locations",
            "/api/stock/movements",
            "/api/suppliers",
            "/health"
        )
    }
)

Write-Host ""
Write-Host "🔍 EXTRACTION DES SPÉCIFICATIONS OPENAPI" -ForegroundColor Yellow

# Créer le répertoire de documentation
$docsPath = "docs/api"
if (!(Test-Path $docsPath)) {
    New-Item -Path $docsPath -ItemType Directory -Force
    Write-Host "📁 Répertoire créé: $docsPath" -ForegroundColor Cyan
}

foreach ($service in $services) {
    Write-Host ""
    Write-Host "📖 Génération de la documentation pour $($service.Name)" -ForegroundColor Cyan
    
    # Création du fichier de documentation par service
    $serviceDoc = @"
# $($service.Name) - Documentation API

## Description
$($service.Description)

## Configuration
- **Port HTTP**: $($service.Port)
- **Port HTTPS**: $($service.Port + 10)
- **Base Path**: /$($service.BasePath -replace '\\', '/')

## Endpoints Disponibles

"@

    foreach ($endpoint in $service.Endpoints) {
        $serviceDoc += "- ``$endpoint``$([Environment]::NewLine)"
    }

    $serviceDoc += @"

## Health Check
- **URL**: http://localhost:$($service.Port)/health
- **Méthode**: GET
- **Réponse**: JSON avec status et service name

## Swagger UI
- **URL**: http://localhost:$($service.Port)/swagger
- **Description**: Interface interactive pour tester les APIs

## Authentication
"@

    if ($service.Name -eq "Auth.API") {
        $serviceDoc += @"
Service d'authentification - génère les tokens JWT pour les autres services.

### Exemple de login:
``````json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password"
}
``````
"@
    } else {
        $serviceDoc += @"
Requiert un token JWT Bearer dans le header Authorization:
``````
Authorization: Bearer <jwt_token>
``````
"@
    }

    $serviceDoc += @"

## Modèles de données
Voir la documentation Swagger pour les schémas complets des modèles.

## Codes d'erreur
- **200**: Succès
- **400**: Requête invalide
- **401**: Non authentifié
- **403**: Non autorisé
- **404**: Ressource non trouvée
- **500**: Erreur serveur

## Contact
Équipe de développement NiesPro ERP
"@

    # Sauvegarde de la documentation
    $fileName = "$docsPath/$($service.Name.ToLower() -replace '\.', '-').md"
    $serviceDoc | Out-File -FilePath $fileName -Encoding UTF8
    Write-Host "✅ Documentation générée: $fileName" -ForegroundColor Green
}

# Création de l'index général
Write-Host ""
Write-Host "📋 CRÉATION DE L'INDEX GÉNÉRAL" -ForegroundColor Yellow

$indexContent = @"
# NiesPro ERP - Documentation API Complète

## Vue d'ensemble
NiesPro est un système ERP (Enterprise Resource Planning) basé sur une architecture microservices avec .NET 8, utilisant les patterns Clean Architecture, DDD (Domain-Driven Design), CQRS et Event Sourcing.

## Architecture Technique
- **.NET 8**: Framework de développement
- **MySQL 8.0**: Base de données principale
- **Redis**: Cache et sessions
- **Entity Framework Core**: ORM avec Pomelo MySQL
- **MediatR**: Implémentation CQRS
- **Serilog**: Logging structuré
- **Docker**: Containerisation
- **Prometheus/Grafana**: Monitoring

## Services Opérationnels

| Service | Port HTTP | Port HTTPS | Description | Status |
|---------|-----------|------------|-------------|---------|
"@

foreach ($service in $services) {
    $indexContent += "| [$($service.Name)](./$(($service.Name.ToLower() -replace '\.', '-')).md) | $($service.Port) | $($service.Port + 10) | $($service.Description) | ✅ Opérationnel |$([Environment]::NewLine)"
}

$indexContent += @"

## Démarrage Rapide

### 1. Avec Docker Compose (Recommandé)
``````bash
# Déploiement production complet
docker compose -f docker-compose.production.yml up -d

# Ou développement
docker compose up -d
``````

### 2. Démarrage Manuel
``````bash
# Démarrer MySQL
docker run -d --name mysql -p 3306:3306 -e MYSQL_ROOT_PASSWORD=NiesPro2024! mysql:8.0

# Démarrer chaque service
cd src/Services/Stock/Stock.API && dotnet run
cd src/Services/Auth/Auth.API && dotnet run
# ... etc
``````

## Tests d'Intégration
``````powershell
# Exécuter les tests automatisés
./scripts/test-services.ps1

# Déploiement production
./scripts/deploy-production.ps1
``````

## URLs de Monitoring

| Service | URL | Credentials |
|---------|-----|-------------|
| API Gateway | http://localhost:5000 | - |
| Prometheus | http://localhost:9090 | - |
| Grafana | http://localhost:3000 | admin/NiesPro2024! |
| Kibana | http://localhost:5601 | - |

## Authentication Flow

1. **Login**: POST ``/api/auth/login`` → Receive JWT token
2. **API Calls**: Include ``Authorization: Bearer <token>`` header
3. **Refresh**: POST ``/api/auth/refresh`` when token expires

## Swagger Documentation
Chaque service expose sa documentation interactive:
- Auth API: http://localhost:5001/swagger
- Catalog API: http://localhost:5003/swagger
- Order API: http://localhost:5002/swagger
- Payment API: http://localhost:5004/swagger
- Stock API: http://localhost:5005/swagger

## Support et Contact
- **Repository**: https://github.com/mikaelarthservices-ux/NiesPro
- **Documentation**: ./docs/
- **Issues**: GitHub Issues
- **Email**: support@niespro.com

## Licence
Copyright © 2024 NiesPro ERP. Tous droits réservés.
"@

$indexContent | Out-File -FilePath "$docsPath/README.md" -Encoding UTF8
Write-Host "✅ Index général créé: $docsPath/README.md" -ForegroundColor Green

# Création d'un fichier de collection Postman
Write-Host ""
Write-Host "📮 GÉNÉRATION DE LA COLLECTION POSTMAN" -ForegroundColor Yellow

$postmanCollection = @{
    info = @{
        name = "NiesPro ERP API Collection"
        description = "Collection complète des APIs NiesPro ERP"
        version = "1.0.0"
        schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    }
    variable = @(
        @{
            key = "baseUrl"
            value = "http://localhost"
            type = "string"
        },
        @{
            key = "authToken"
            value = ""
            type = "string"
        }
    )
    item = @()
}

foreach ($service in $services) {
    $serviceFolder = @{
        name = $service.Name
        description = $service.Description
        item = @()
    }
    
    # Health check pour chaque service
    $healthCheck = @{
        name = "Health Check"
        request = @{
            method = "GET"
            header = @()
            url = @{
                raw = "{{baseUrl}}:$($service.Port)/health"
                host = @("{{baseUrl}}")
                port = "$($service.Port)"
                path = @("health")
            }
        }
    }
    $serviceFolder.item += $healthCheck
    
    # Ajouter d'autres endpoints selon le service
    foreach ($endpoint in $service.Endpoints) {
        if ($endpoint -ne "/health") {
            $apiCall = @{
                name = "$(($endpoint -split '/')[-1]) API"
                request = @{
                    method = "GET"
                    header = @(
                        @{
                            key = "Authorization"
                            value = "Bearer {{authToken}}"
                            type = "text"
                        }
                    )
                    url = @{
                        raw = "{{baseUrl}}:$($service.Port)$endpoint"
                        host = @("{{baseUrl}}")
                        port = "$($service.Port)"
                        path = ($endpoint -split '/' | Where-Object { $_ -ne "" })
                    }
                }
            }
            $serviceFolder.item += $apiCall
        }
    }
    
    $postmanCollection.item += $serviceFolder
}

$postmanJson = $postmanCollection | ConvertTo-Json -Depth 10
$postmanJson | Out-File -FilePath "$docsPath/NiesPro-ERP-Postman-Collection.json" -Encoding UTF8
Write-Host "✅ Collection Postman générée: $docsPath/NiesPro-ERP-Postman-Collection.json" -ForegroundColor Green

Write-Host ""
Write-Host "📚 DOCUMENTATION GÉNÉRÉE AVEC SUCCÈS!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Fichiers créés:" -ForegroundColor White
Get-ChildItem $docsPath -Recurse | ForEach-Object {
    Write-Host "  • $($_.FullName)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "🌐 Pour voir la documentation:" -ForegroundColor Yellow
Write-Host "  1. Ouvrir: $docsPath/README.md" -ForegroundColor Cyan
Write-Host "  2. Importer dans Postman: $docsPath/NiesPro-ERP-Postman-Collection.json" -ForegroundColor Cyan
Write-Host "  3. Swagger UI: http://localhost:[port]/swagger (quand les services sont démarrés)" -ForegroundColor Cyan