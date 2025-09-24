# DOCUMENTATION DES PORTS - NIESPRO ERP

## Architecture des Microservices - Ports Standards

### Services Principaux

| Service | Port HTTP | Port HTTPS | Base URL Gateway | Status |
|---------|-----------|------------|------------------|--------|
| **Gateway.API** | 5000 | 5010 | - | 🟢 Principal |
| **Auth.API** | 5001 | 5011 | https://localhost:5011 | 🟢 Critique |
| **Order.API** | 5002 | 5012 | https://localhost:5012 | 🟢 Actif |
| **Catalog.API** | 5003 | 5013 | https://localhost:5013 | 🟢 Actif |
| **Payment.API** | 5004 | 5014 | https://localhost:5014 | 🟢 Actif |
| **Stock.API** | 5005 | 5006 | https://localhost:5006 | 🟢 Actif |
| **Restaurant.API** | 7001 | 7011 | https://localhost:7011 | 🟢 Actif |
| **Customer.API** | 8001 | 8011 | http://localhost:8001 | 🟢 Actif |

### Services de Base de Données

| Service | Port | Description |
|---------|------|-------------|
| **MySQL** | 3306 | Base de données principale |
| **Redis** | 6379 | Cache et sessions |

### Conventions et Standards

#### 1. **Conventions de Nommage des Ports**
- **Gateway** : 5000 (HTTP) / 5010 (HTTPS)
- **Services Métier** : 50XX (HTTP) / 501X (HTTPS)
- **Services Spécialisés** : 
  - Restaurant : 7001/7011
  - Customer : 8001/8011

#### 2. **Configuration Standard**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:XXXX"
      },
      "Https": {
        "Url": "https://localhost:XXXX"
      }
    }
  }
}
```

#### 3. **Protocoles Gateway**
- **Auth.API** : HTTPS uniquement (sécurité)
- **Customer.API** : HTTP (service public)
- **Autres services** : HTTPS (sécurité métier)

### Répartition des Bases de Données

| Service | Base de Données | Port MySQL |
|---------|----------------|------------|
| Auth.API | NiesPro_Auth | 3306 |
| Customer.API | NiesPro_Customer | 3306 |
| Catalog.API | NiesPro_Catalog | 3306 |
| Order.API | NiesPro_Order + NiesPro_OrderEventStore | 3306 |
| Payment.API | NiesPro_Payment | 3306 |
| Stock.API | NiesPro_Stock | 3306 |
| Restaurant.API | NiesPro_Restaurant | 3306 |

### Scripts de Validation

#### **validate-configurations.ps1**
Vérifie l'alignement entre :
- Configuration des services (appsettings.json)
- Configuration du Gateway
- Standards documentés

#### **check-services.ps1**
Vérifie l'état runtime :
- Ports ouverts
- Processus actifs
- Health checks

#### **diagnostic-services.ps1**
Diagnostic complet :
- Connectivité des services
- Bases de données
- Tests fonctionnels

### Commandes de Validation

```powershell
# Validation des configurations
.\tests\Scripts\validate-configurations.ps1

# Vérification de l'état des services
.\tests\Scripts\check-services.ps1

# Diagnostic complet
.\tests\Scripts\diagnostic-services.ps1
```

### Dépannage

#### **Conflit de Ports**
1. Vérifier que launchSettings.json n'existe pas (sauf développement local)
2. Utiliser uniquement la configuration Kestrel dans appsettings.json
3. Préférer `localhost` à `0.0.0.0` pour la cohérence

#### **Problèmes Gateway**
1. Vérifier que les URLs dans Gateway/appsettings.json correspondent
2. Respecter HTTP vs HTTPS selon les conventions
3. Utiliser les scripts de validation régulièrement

#### **Base de Données**
1. Toutes les bases utilisent MySQL sur port 3306
2. Convention de nommage : `NiesPro_{ServiceName}`
3. EventStore séparé pour Order.API

---

**Dernière mise à jour** : 23/09/2025
**Validé par** : Script validate-configurations.ps1
**Status** : ✅ Toutes les configurations alignées