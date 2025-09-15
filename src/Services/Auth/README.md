# 🔐 NiesPro Auth Service

Service d'authentification et d'autorisation pour le système ERP NiesPro.

## 📁 Structure du Projet

```
src/Services/Auth/
├── Auth.API/                 # API Web (contrôleurs, middleware, configuration)
├── Auth.Application/         # Logique métier (CQRS, handlers, DTOs)
├── Auth.Domain/             # Entités de domaine, interfaces, règles métier
└── Auth.Infrastructure/     # Accès aux données, repositories, services externes
```

## 🏗️ Architecture

- **Clean Architecture** avec séparation des couches
- **CQRS** avec MediatR pour la séparation des commandes et requêtes
- **Domain-Driven Design** pour la modélisation métier
- **Repository Pattern** pour l'accès aux données
- **Entity Framework Core** avec MySQL
- **JWT + Device Keys** pour l'authentification sécurisée

## 🔧 Technologies

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - API Web
- **Entity Framework Core** - ORM
- **MySQL 8.0** - Base de données
- **Redis** - Cache et sessions
- **Serilog** - Logging structuré
- **Swagger/OpenAPI** - Documentation API
- **Docker** - Conteneurisation

## 🚀 Démarrage Rapide

### Prérequis
- .NET 8.0 SDK
- Docker et Docker Compose
- MySQL (ou utiliser Docker)

### 1. Cloner et configurer
```bash
git clone <repository>
cd NiesPro/src/Services/Auth/Auth.API
```

### 2. Configuration
Copier `appsettings.json` et modifier les chaînes de connexion si nécessaire.

### 3. Base de données
```bash
# Avec Docker (recommandé)
docker-compose up mysql redis -d

# Créer les migrations
dotnet ef migrations add InitialCreate --startup-project Auth.API --project Auth.Infrastructure

# Appliquer les migrations
dotnet ef database update --startup-project Auth.API --project Auth.Infrastructure
```

### 4. Lancer l'API
```bash
dotnet run --project Auth.API
```

L'API sera disponible sur : http://localhost:5000 (ou https://localhost:5001)

## 📊 Endpoints Principaux

### Authentification
- `POST /api/auth/login` - Connexion utilisateur
- `POST /api/auth/refresh-token` - Rafraîchissement du token
- `POST /api/auth/logout` - Déconnexion
- `POST /api/auth/register` - Inscription

### Gestion des appareils
- `POST /api/auth/register-device` - Enregistrement d'appareil
- `GET /api/auth/devices` - Liste des appareils de l'utilisateur

### Profil utilisateur
- `GET /api/users/profile` - Profil de l'utilisateur connecté
- `PUT /api/users/profile` - Mise à jour du profil
- `POST /api/users/change-password` - Changement de mot de passe

### Administration (Admin uniquement)
- `GET /api/users` - Liste de tous les utilisateurs
- `GET /api/users/{id}` - Détails d'un utilisateur

## 🔒 Sécurité

### Système d'authentification dual
1. **JWT Tokens** - Authentification stateless standard
2. **Device Keys** - Validation supplémentaire des appareils autorisés

### Fonctionnalités de sécurité
- Hachage des mots de passe avec BCrypt
- Limitation des tentatives de connexion
- Validation des appareils
- Audit trail complet
- Gestion des sessions utilisateur

## 🗃️ Modèle de Données

### Entités principales
- **User** - Utilisateurs du système
- **Role** - Rôles (Admin, Manager, User, etc.)
- **Permission** - Permissions granulaires
- **Device** - Appareils autorisés
- **UserSession** - Sessions actives
- **AuditLog** - Journal d'audit

## 🔧 Configuration

### Variables d'environnement
```env
ConnectionStrings__DefaultConnection=Server=localhost;Database=niespro_auth;...
ConnectionStrings__Redis=localhost:6379
JwtSettings__SecretKey=your-secret-key
JwtSettings__ExpirationInMinutes=60
DeviceSettings__RequireDeviceValidation=true
```

## 🧪 Tests

```bash
# Tests unitaires
dotnet test Auth.Domain.Tests

# Tests d'intégration
dotnet test Auth.API.Tests
```

## 📝 Logging

Les logs sont configurés avec Serilog et envoyés vers :
- Console (développement)
- Fichiers rotatifs (`logs/auth-api-*.log`)
- Seq (si configuré)

## 🐳 Docker

### Build de l'image
```bash
docker build -t niespro-auth-api -f Auth.API/Dockerfile .
```

### Lancer avec Docker Compose
```bash
docker-compose up auth-api
```

## 🔄 Migrations

### Créer une nouvelle migration
```bash
dotnet ef migrations add MigrationName --startup-project Auth.API --project Auth.Infrastructure
```

### Appliquer les migrations
```bash
dotnet ef database update --startup-project Auth.API --project Auth.Infrastructure
```

## 📖 Documentation API

Swagger UI disponible sur : `/swagger` quand l'API est en cours d'exécution.

## 🛠️ Développement

### Structure des handlers CQRS
```csharp
// Commande
public class LoginCommand : IRequest<ApiResponse<LoginResponse>>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// Handler
public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    // Implémentation...
}
```

### Ajout d'une nouvelle fonctionnalité
1. Créer les DTOs dans `Application/Features/`
2. Ajouter les entités de domaine si nécessaire
3. Implémenter les handlers CQRS
4. Ajouter les endpoints dans les contrôleurs
5. Créer les tests

## 🤝 Contribution

1. Fork le projet
2. Créer une branche feature (`git checkout -b feature/AmazingFeature`)
3. Commit les changements (`git commit -m 'Add some AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.