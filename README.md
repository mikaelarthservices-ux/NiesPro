# 🎯 NIESPRO ERP - Solution Complète Boutique & Restaurant

[![.NET Version](https://img.shields.io/badge/.NET-8.0+-blue)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0+-orange)](https://mysql.com/)
[![Microservices](https://img.shields.io/badge/Architecture-Microservices-green)](SERVICES-MATRIX.md)
[![License](https://img.shields.io/badge/License-Proprietary-red)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Production%20Ready-brightgreen)](SERVICES-MATRIX.md)

## 📋 **DOCUMENTATION CENTRALE**

> **📚 [VOIR LA DOCUMENTATION COMPLÈTE](./DOCUMENTATION-CENTRALE.md)**

| Document Principal | Description |
|-------------------|-------------|
| **[🎯 DOCUMENTATION CENTRALE](./DOCUMENTATION-CENTRALE.md)** | Index complet et navigation |
| **[📊 MATRICE DES SERVICES](./SERVICES-MATRIX.md)** | Services, dépendances et statuts |
| **[🔧 ARCHITECTURE MICROSERVICES](./ARCHITECTURE-MICROSERVICES.md)** | Architecture technique détaillée |
| **[📋 CAHIER DES CHARGES UNIFIÉ](./CAHIER-CHARGES-UNIFIE.md)** | Spécifications complètes |
| **[⚙️ CONFIGURATION FINALE](./CONFIGURATION-FINALE.md)** | Ports, environnements et déploiement |

## 🚀 **DÉMARRAGE RAPIDE**

### **1. Prérequis**
- .NET 8 SDK
- MySQL 8.0+
- Git

### **2. Installation**
```bash
git clone https://github.com/mikaelarthservices-ux/NiesPro.git
cd NiesPro
.\scripts\setup-databases.ps1
.\start-all-services.ps1
```

### **3. Vérification**
- Gateway: https://localhost:5010/swagger
- Services Health: https://localhost:5010/health

## 🎯 **DESCRIPTION EXÉCUTIVE**

NiesPro ERP est une **solution enterprise** de gestion intégrée pour boutiques et restaurants, construite sur une **architecture microservices moderne** avec .NET 8. Le système garantit une sécurité maximale, une scalabilité élevée et une expérience utilisateur optimale.

## ✨ Fonctionnalités principales

### 📊 **LOGGING CENTRALISÉ** ✅ **INTÉGRÉ**
- **Service Logs** : API centralisée (port 5018) avec MySQL + Elasticsearch
- **NiesPro.Logging.Client** : Bibliothèque partagée pour tous les services
- **Audit trail automatique** : Traçabilité complète des actions CUD
- **Monitoring en temps réel** : Métriques et alertes système
- **Recherche avancée** : Full-text search avec Elasticsearch
- **Status intégration** : Auth ✅ 100% | Catalog ✅ 100% | Order 🔄 Prochaine | Autres ❌ À faire

### 🏪 Gestion Boutique
- Catalogue produits avec variantes et codes-barres
- Multi-magasin et points de vente (POS)
- Intégration lecteur code-barres et imprimante tickets
- Gestion des stocks en temps réel

### 🍽️ Gestion Restaurant
- Menus, plats et combos personnalisables
- Gestion des tables et plan de salle interactif
- Prise de commande mobile/tablette
- Écran cuisine temps réel
- Livraison et commandes à emporter

### 📊 Modules transversaux
- **Stock & Approvisionnement** : inventaires, fournisseurs, alertes
- **Caisse & Paiements** : multi-moyens, multi-devises, dettes clients
- **Clients & Fidélité** : historique, points, promotions, marketing SMS/WhatsApp
- **Reporting** : dashboards interactifs, exports Excel/PDF, prévisions
- **Notifications** : internes (cuisine, stock) et externes (clients)

## 🏗️ Architecture technique

### Technologies
- **Backend** : .NET 6+ (C#), ASP.NET Core Web API
- **Frontend** : WPF (Desktop), MAUI (Mobile), Blazor (Web)
- **Base de données** : MySQL 8.0+
- **Authentification** : JWT + Device Keys
- **Design** : Material Design avec Design System centralisé
- **Communication** : REST APIs, SignalR (temps réel)

### Microservices
- **Auth Service** : Authentification et autorisation
- **Product Service** : Catalogue et variantes
- **Stock Service** : Inventaires et mouvements
- **Order Service** : Commandes et facturation
- **Payment Service** : Transactions et moyens de paiement
- **Customer Service** : Clients et fidélité
- **Notification Service** : SMS, WhatsApp, notifications internes
- **File Service** : Gestion centralisée des fichiers
- **Log Service** : Audit et traçabilité
- **Report Service** : Statistiques et exports

## 🔐 Sécurité

- **Double authentification** : Login/mot de passe + empreinte terminal
- **Gestion des rôles** : Admin, Manager, Serveur, Caissier, Stockiste
- **Chiffrement** : HTTPS, JWT, chiffrement des données sensibles
- **Audit complet** : ✅ **LOGS CENTRALISÉS INTÉGRÉS** (Service Logs + Client partagé)
- **Mode offline** : Synchronisation sécurisée au retour

## 🚀 Installation

### Prérequis
- .NET 6.0 SDK ou supérieur
- MySQL 8.0+
- Visual Studio 2022 ou VS Code
- Node.js 16+ (pour les outils de build)

### Configuration
1. Cloner le repository
```bash
git clone https://github.com/votre-org/NiesPro.git
cd NiesPro
```

2. Configurer la base de données
```bash
# Créer la base de données
mysql -u root -p < scripts/database/init.sql

# Mettre à jour la chaîne de connexion dans appsettings.json
```

3. Installer les dépendances
```bash
dotnet restore
npm install
```

4. Lancer les microservices
```bash
# Démarrer tous les services
docker-compose up -d

# Ou démarrer individuellement
dotnet run --project src/Services/Auth/Auth.API
dotnet run --project src/Services/Product/Product.API
# ... autres services
```

## 📱 Applications clientes

### Desktop (WPF)
```bash
dotnet run --project src/Clients/Desktop/NiesPro.Desktop
```

### Mobile (MAUI)
```bash
dotnet build src/Clients/Mobile/NiesPro.Mobile -f net6.0-android
dotnet build src/Clients/Mobile/NiesPro.Mobile -f net6.0-ios
```

### Web (Blazor)
```bash
dotnet run --project src/Clients/Web/NiesPro.Web
```

## 🧪 Tests et Qualité

### État d'avancement des tests par service

| Service | Tests Unitaires | Tests Intégration | Couverture | Status |
|---------|----------------|-------------------|-------------|---------|
| **Catalog** | ✅ 100% (Complet) | ✅ 70% endpoints | 85%+ | 🎯 **PRODUCTION READY** |
| **Auth** | ✅ 100% (41 tests) | ✅ Infrastructure complète | 85%+ | 🎯 **PRODUCTION READY** |
| Customer | 🚧 En cours | ❌ À créer | - | 🔄 **EN DÉVELOPPEMENT** |
| Restaurant | 🚧 En cours | ❌ À créer | - | 🔄 **EN DÉVELOPPEMENT** |
| Order | ❌ À créer | ❌ À créer | - | ⏳ **PLANIFIÉ** |
| Payment | ❌ À créer | ❌ À créer | - | ⏳ **PLANIFIÉ** |

### Commandes de test

```bash
# Tests Catalog (COMPLETS)
dotnet test tests/Catalog/Unit/Catalog.Tests.Unit.csproj
./tests/Catalog/run-tests.ps1

# Tests Auth (COMPLETS)  
dotnet test tests/Auth/Unit/Auth.Tests.Unit.csproj
./tests/Auth/run-tests.ps1

# Scripts d'automatisation disponibles
./tools/catalog-service-tester.ps1    # Tests automatisés Catalog
./tools/catalog-db-inspector.ps1      # Validation DB Catalog
```

### Standards de qualité adoptés
- ✅ **Tests unitaires** : NUnit + FluentAssertions + Moq + AutoFixture
- ✅ **Tests d'intégration** : ASP.NET Core Testing + TestContainers
- ✅ **Documentation complète** : README + Status + Scripts pour chaque service
- ✅ **Automatisation** : Scripts PowerShell pour exécution et rapports
- ✅ **CI/CD Ready** : Infrastructure compatible pipelines

## 📖 Documentation

- [Architecture détaillée](docs/ARCHITECTURE.md)
- [Guide de développement](docs/DEVELOPMENT.md)
- [API Documentation](docs/API.md)
- [Guide d'installation](docs/INSTALLATION.md)
- [Guide utilisateur](docs/USER_GUIDE.md)

## 🤝 Contribution

1. Fork le projet
2. Créer une branche feature (`git checkout -b feature/AmazingFeature`)
3. Commit les changements (`git commit -m 'Add AmazingFeature'`)
4. Push sur la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

Voir [CONTRIBUTING.md](CONTRIBUTING.md) pour plus de détails.

## 📊 Roadmap et avancement

### Services Microservices
- [x] **Auth Service** : ✅ **COMPLET** - Authentification + Tests professionnels (41 tests, 100% succès)
- [x] **Catalog Service** : ✅ **COMPLET** - Catalogue produits + Tests professionnels (100% succès)  
- [x] **Infrastructure de tests** : ✅ **DÉPLOYÉE** - Standards professionnels pour tous services
- [ ] **Customer Service** : 🚧 **EN COURS** - Prochaine étape (tests à implémenter)
- [ ] **Restaurant Service** : ⏳ Planifié
- [ ] **Order Service** : ⏳ Planifié  
- [ ] **Payment Service** : ⏳ Planifié
- [ ] **Stock Service** : ⏳ Planifié

### Phases de développement
- [x] **Phase 1** : Architecture et authentification ✅
- [x] **Phase 2a** : Service Catalog + Infrastructure tests ✅
- [x] **Phase 2b** : Service Auth + Tests complets ✅
- [ ] **Phase 3** : Service Customer + Restaurant 🚧
- [ ] **Phase 4** : Services Order + Payment ⏳
- [ ] **Phase 5** : Reporting et analytics ⏳
- [ ] **Phase 6** : Mobile et notifications ⏳
- [ ] **Phase 7** : Intégrations externes ⏳

### Métriques de qualité actuelles
- **Services en production** : 2/7 (Auth, Catalog)
- **Tests unitaires** : 2 services avec 100% de succès
- **Infrastructure complète** : Déployée et réutilisable
- **Documentation** : Standards professionnels établis

## 📄 License

Ce projet est sous licence propriétaire. Voir [LICENSE](LICENSE) pour plus de détails.

## 👥 Équipe

- **Lead Developer** : [Nom]
- **Architecte** : [Nom]
- **UI/UX Designer** : [Nom]
- **DevOps** : [Nom]

## 📞 Support

- **Email** : support@niespro.com
- **Documentation** : https://docs.niespro.com
- **Issues** : https://github.com/votre-org/NiesPro/issues

---

**NiesPro ERP** - La solution complète pour votre business ! 🚀
