# NiesPro ERP - Système de gestion Boutique & Restaurant

[![.NET Version](https://img.shields.io/badge/.NET-6.0+-blue)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0+-orange)](https://mysql.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-green)](BUILD)

## 🎯 Description

NiesPro ERP est une solution complète de gestion pour boutiques et restaurants, développée en C# avec une architecture microservices moderne. Le système offre une sécurité renforcée avec double authentification (utilisateur + terminal) et une interface utilisateur cohérente basée sur Material Design.

## ✨ Fonctionnalités principales

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
- **Audit complet** : Logs centralisés de toutes les actions
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

## 🧪 Tests

```bash
# Tests unitaires
dotnet test tests/Unit/

# Tests d'intégration
dotnet test tests/Integration/

# Tests E2E
dotnet test tests/E2E/
```

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

## 📊 Roadmap

- [x] **Phase 1** : Architecture et authentification
- [x] **Phase 2** : Modules boutique et stock
- [ ] **Phase 3** : Modules restaurant et caisse
- [ ] **Phase 4** : Reporting et analytics
- [ ] **Phase 5** : Mobile et notifications
- [ ] **Phase 6** : Intégrations externes

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
