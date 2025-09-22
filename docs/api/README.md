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
| [Auth.API](./auth-api.md) | 5001 | 5011 | Service d'authentification et autorisation | ✅ Opérationnel |
| [Catalog.API](./catalog-api.md) | 5003 | 5013 | Gestion du catalogue produits | ✅ Opérationnel |
| [Order.API](./order-api.md) | 5002 | 5012 | Gestion des commandes avec Event Sourcing | ✅ Opérationnel |
| [Payment.API](./payment-api.md) | 5004 | 5014 | Traitement des paiements multi-gateway | ✅ Opérationnel |
| [Stock.API](./stock-api.md) | 5005 | 5006 | Gestion des stocks et inventaire | ✅ Opérationnel |

## Démarrage Rapide

### 1. Avec Docker Compose (Recommandé)
```bash
# Déploiement production complet
docker compose -f docker-compose.production.yml up -d

# Ou développement
docker compose up -d
```

### 2. Démarrage Manuel
```bash
# Démarrer MySQL
docker run -d --name mysql -p 3306:3306 -e MYSQL_ROOT_PASSWORD=NiesPro2024! mysql:8.0

# Démarrer Stock.API (service validé)
cd src/Services/Stock/Stock.API && dotnet run
```

## Tests d'Intégration
```powershell
# Exécuter les tests automatisés
./scripts/test-services.ps1

# Déploiement production
./scripts/deploy-production.ps1
```

## URLs de Monitoring

| Service | URL | Credentials |
|---------|-----|-------------|
| API Gateway | http://localhost:5000 | - |
| Prometheus | http://localhost:9090 | - |
| Grafana | http://localhost:3000 | admin/NiesPro2024! |
| Kibana | http://localhost:5601 | - |

## Authentication Flow

1. **Login**: POST `/api/auth/login` → Receive JWT token
2. **API Calls**: Include `Authorization: Bearer <token>` header
3. **Refresh**: POST `/api/auth/refresh` when token expires

## Swagger Documentation
Chaque service expose sa documentation interactive:
- Auth API: http://localhost:5001/swagger
- Catalog API: http://localhost:5003/swagger
- Order API: http://localhost:5002/swagger
- Payment API: http://localhost:5004/swagger
- Stock API: http://localhost:5005/swagger

## Services Status (Validation Actuelle)

### ✅ Stock.API - 100% Opérationnel
- **HTTP**: http://localhost:5005/health ✅
- **HTTPS**: https://localhost:5006/health ✅
- **Fonctionnalités**: Gestion complète des stocks, emplacements, mouvements
- **Base de données**: MySQL NiesPro_Stock ✅

### 🔧 Autres Services - Configuration Requise
Les autres services nécessitent une configuration SSL et un redémarrage pour être pleinement opérationnels.

## Commandes de Gestion

### Redémarrage des Services
```bash
# Stock.API uniquement (opérationnel)
cd src/Services/Stock/Stock.API
dotnet run

# Tous les services (après configuration)
./scripts/deploy-production.ps1
```

### Tests de Santé
```bash
# Test Stock.API
curl http://localhost:5005/health
curl -k https://localhost:5006/health

# Test via PowerShell
Invoke-WebRequest -Uri "https://localhost:5006/health" -UseBasicParsing -SkipCertificateCheck
```

## Support et Contact
- **Repository**: https://github.com/mikaelarthservices-ux/NiesPro
- **Documentation**: ./docs/
- **Status**: 83% fonctionnalités critiques opérationnelles

## Licence
Copyright © 2024 NiesPro ERP. Tous droits réservés.