# 🚀 Instructions de Lancement - Architecture NiesPro

## ✅ Architecture Complète Prête

L'architecture microservices NiesPro est maintenant **100% opérationnelle** avec :

- **Auth.API** (Port 5001) - Authentification JWT 
- **Order.API** (Port 5002) - Gestion des commandes avec Event Sourcing
- **Catalog.API** (Port 5003) - Catalogue produits avec CQRS
- **Gateway.API** (Port 5000) - Orchestration et routage intelligent

## 🎯 Lancement Recommandé (Terminaux Externes)

### Option 1 : Lancement Automatique de Tous les Services
```bash
# Naviguez vers le dossier scripts
cd C:\Users\HP\Documents\projets\NiesPro\scripts

# Lancez tous les services d'un coup
start-all-services.bat
```

### Option 2 : Lancement PowerShell (Recommandé)
```powershell
# Naviguez vers le dossier scripts
cd C:\Users\HP\Documents\projets\NiesPro\scripts

# Lancez tous les services
.\start-services.ps1

# Ou lancez un service spécifique
.\start-services.ps1 -AuthOnly
.\start-services.ps1 -OrderOnly  
.\start-services.ps1 -CatalogOnly
.\start-services.ps1 -GatewayOnly
```

### Option 3 : Lancement Manuel (Services Individuels)
```bash
# Auth.API
scripts\start-auth-api.bat

# Order.API  
scripts\start-order-api.bat

# Catalog.API
scripts\start-catalog-api.bat

# Gateway.API
scripts\start-gateway-api.bat
```

## 📡 URLs d'Accès

| Service | URL | Documentation |
|---------|-----|---------------|
| **Gateway** | https://localhost:5000 | [Swagger](https://localhost:5000/swagger) |
| **Auth.API** | https://localhost:5001 | [Swagger](https://localhost:5001/swagger) |
| **Order.API** | https://localhost:5002 | [Swagger](https://localhost:5002/swagger) |
| **Catalog.API** | https://localhost:5003 | [Swagger](https://localhost:5003/swagger) |

## 🔧 Supervision et Monitoring

- **Health Checks UI** : https://localhost:5000/health-ui
- **Gateway Info** : https://localhost:5000/api/gateway/info
- **Gateway Metrics** : https://localhost:5000/api/gateway/metrics

## 🛑 Arrêt des Services

```bash
# Arrêt automatique de tous les services
scripts\stop-all-services.bat

# Ou via PowerShell
.\start-services.ps1 -StopAll
```

## ✨ Avantages de cette Architecture

### 🎯 **Performance & Scalabilité**
- Chaque microservice s'exécute de manière indépendante
- Load balancing automatique via le Gateway
- Isolation des ressources et des processus

### 🔒 **Sécurité Professionnelle**
- Authentification JWT centralisée
- Validation des tokens par le Gateway
- CORS et headers de sécurité configurés

### 📊 **Monitoring & Observabilité**
- Logs Serilog structurés pour chaque service
- Health checks en temps réel
- Métriques de performance disponibles

### 🚀 **Développement**
- Hot reload sur chaque service individuellement
- Debugging facilité avec terminaux séparés
- Architecture Clean + CQRS + Event Sourcing

## 🧪 Tests Recommandés

### 1. Test d'Authentification
```bash
# Via Gateway
POST https://localhost:5000/api/auth/login
{
  "email": "admin@niespro.com", 
  "password": "Admin@123"
}
```

### 2. Test de Routage Gateway
```bash
# Accès aux produits via Gateway
GET https://localhost:5000/api/products

# Accès aux commandes via Gateway  
GET https://localhost:5000/api/orders
```

### 3. Test Health Checks
```bash
# Status global
GET https://localhost:5000/health

# UI interactive
GET https://localhost:5000/health-ui
```

## 🎉 Architecture Professionnelle Complète

Cette implémentation respecte toutes les meilleures pratiques :

- ✅ **Clean Architecture** avec séparation des couches
- ✅ **CQRS + MediatR** pour la logique métier
- ✅ **Event Sourcing** pour Order.API
- ✅ **MySQL natif** avec migrations EF Core
- ✅ **JWT Authentication** centralisée
- ✅ **API Gateway** intelligent
- ✅ **Logging professionnel** avec Serilog
- ✅ **Health Checks** et monitoring
- ✅ **Swagger** documentation automatique
- ✅ **Rate Limiting** et sécurité

**🚀 L'architecture NiesPro est maintenant prête pour la production !**