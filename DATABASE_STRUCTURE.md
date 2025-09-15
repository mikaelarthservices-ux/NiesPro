# 📊 Structure de la Base de Données NiesPro

## 🗄️ Localisation de la Base de Données

La base de données est configurée dans **Docker avec MySQL 8.0**. Voici comment y accéder :

### Configuration Docker
```yaml
# Dans docker-compose.yml
mysql:
  image: mysql:8.0
  container_name: niespro-mysql
  environment:
    MYSQL_ROOT_PASSWORD: NiesPro2025!Root
    MYSQL_DATABASE: NiesPro_Auth
  ports:
    - "3306:3306"
```

### Connexion à la Base de Données
- **Host**: localhost
- **Port**: 3306
- **Utilisateur**: root
- **Mot de passe**: NiesPro2025!Root
- **Base de données principale**: NiesPro_Auth

## 📋 Structure des Bases de Données

### 🎯 Architecture Multi-Base
Le système utilise une approche **Database per Service** avec 8 bases séparées :

```
├── NiesPro_Auth        # Authentification et utilisateurs
├── NiesPro_Product     # Catalogue produits
├── NiesPro_Stock       # Gestion des stocks
├── NiesPro_Order       # Commandes et ventes
├── NiesPro_Payment     # Paiements et facturation
├── NiesPro_Customer    # Clients et prospects
├── NiesPro_Restaurant  # Spécifique restaurant (tables, menus)
└── NiesPro_Log         # Logs et audit
```

## 🔐 Base de Données Auth (Principale)

### Tables et Relations

#### 👤 **Users** (Utilisateurs)
```sql
CREATE TABLE Users (
    Id CHAR(36) PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    EmailConfirmed BOOLEAN DEFAULT FALSE,
    PhoneNumber VARCHAR(20),
    FailedLoginAttempts INT DEFAULT 0,
    LockoutEnd DATETIME NULL,
    LastLoginAt DATETIME NULL,
    LastLoginIp VARCHAR(45),
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    CreatedBy CHAR(36),
    UpdatedBy CHAR(36)
);
```

#### 🎭 **Roles** (Rôles)
```sql
CREATE TABLE Roles (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(50) UNIQUE NOT NULL,
    Description TEXT,
    IsSystemRole BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);
```

#### 🔑 **Permissions** (Permissions)
```sql
CREATE TABLE Permissions (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT,
    Category VARCHAR(50),
    CreatedAt DATETIME NOT NULL
);
```

#### 🔗 **UserRoles** (Utilisateurs-Rôles)
```sql
CREATE TABLE UserRoles (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    RoleId CHAR(36) NOT NULL,
    AssignedAt DATETIME NOT NULL,
    AssignedBy CHAR(36),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    UNIQUE KEY (UserId, RoleId)
);
```

#### 🔗 **RolePermissions** (Rôles-Permissions)
```sql
CREATE TABLE RolePermissions (
    Id CHAR(36) PRIMARY KEY,
    RoleId CHAR(36) NOT NULL,
    PermissionId CHAR(36) NOT NULL,
    GrantedAt DATETIME NOT NULL,
    GrantedBy CHAR(36),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    UNIQUE KEY (RoleId, PermissionId)
);
```

#### 📱 **Devices** (Appareils Autorisés)
```sql
CREATE TABLE Devices (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    DeviceKey CHAR(64) UNIQUE NOT NULL,
    DeviceName VARCHAR(100) NOT NULL,
    DeviceType VARCHAR(50) NOT NULL, -- Mobile, Desktop, Tablet
    DeviceInfo TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    LastUsedAt DATETIME,
    LastIpAddress VARCHAR(45),
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

#### 🔄 **UserSessions** (Sessions Utilisateur)
```sql
CREATE TABLE UserSessions (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    DeviceId CHAR(36),
    RefreshToken VARCHAR(255) UNIQUE NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    CreatedAt DATETIME NOT NULL,
    RevokedAt DATETIME,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE SET NULL
);
```

#### 📝 **AuditLogs** (Journal d'Audit)
```sql
CREATE TABLE AuditLogs (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36),
    Action VARCHAR(100) NOT NULL,
    EntityName VARCHAR(100),
    EntityId CHAR(36),
    OldValues JSON,
    NewValues JSON,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);
```

## 🔍 Index et Performances

### Index Principaux
```sql
-- Performance pour les connexions
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

-- Performance pour les sessions
CREATE INDEX IX_UserSessions_UserId ON UserSessions(UserId);
CREATE INDEX IX_UserSessions_RefreshToken ON UserSessions(RefreshToken);
CREATE INDEX IX_UserSessions_ExpiresAt ON UserSessions(ExpiresAt);

-- Performance pour les appareils
CREATE INDEX IX_Devices_UserId ON Devices(UserId);
CREATE INDEX IX_Devices_DeviceKey ON Devices(DeviceKey);

-- Performance pour l'audit
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
```

## 🎯 Données Initiales (Seed Data)

### Rôles Système
```sql
INSERT INTO Roles (Id, Name, Description, IsSystemRole) VALUES
('admin-role-id', 'SUPER_ADMIN', 'Super Administrateur', TRUE),
('manager-role-id', 'ADMIN', 'Administrateur', TRUE),
('cashier-role-id', 'MANAGER', 'Manager', TRUE),
('waiter-role-id', 'CASHIER', 'Caissier', TRUE),
('user-role-id', 'USER', 'Utilisateur', TRUE);
```

### Permissions de Base
```sql
INSERT INTO Permissions (Id, Name, Description, Category) VALUES
('perm-user-read', 'USER_READ', 'Lire les utilisateurs', 'USER'),
('perm-user-write', 'USER_WRITE', 'Modifier les utilisateurs', 'USER'),
('perm-product-read', 'PRODUCT_READ', 'Lire les produits', 'PRODUCT'),
('perm-product-write', 'PRODUCT_WRITE', 'Modifier les produits', 'PRODUCT'),
('perm-order-read', 'ORDER_READ', 'Lire les commandes', 'ORDER'),
('perm-order-write', 'ORDER_WRITE', 'Créer des commandes', 'ORDER');
```

## 🛠️ Outils d'Accès à la Base

### 1. **Adminer** (Interface Web)
- URL: http://localhost:8080 (quand Docker est démarré)
- Serveur: mysql
- Utilisateur: root
- Mot de passe: NiesPro2025!Root

### 2. **MySQL Workbench**
- Host: localhost:3306
- Username: root
- Password: NiesPro2025!Root

### 3. **Ligne de Commande**
```bash
# Connexion directe au conteneur
docker exec -it niespro-mysql mysql -u root -p

# Ou via MySQL client local
mysql -h localhost -P 3306 -u root -p
```

## 📈 État Actuel

### ✅ Configuré
- ✅ Script d'initialisation SQL
- ✅ Configuration Docker Compose
- ✅ Entités Entity Framework
- ✅ DbContext et configurations

### ⚠️ À Faire
- 🔄 Créer les migrations Entity Framework
- 🔄 Démarrer les conteneurs Docker
- 🔄 Appliquer les migrations
- 🔄 Insérer les données initiales

## 🚀 Prochaines Étapes

1. **Démarrer MySQL**: `docker-compose up -d mysql`
2. **Créer les migrations**: `dotnet ef migrations add InitialCreate`
3. **Appliquer les migrations**: `dotnet ef database update`
4. **Vérifier dans Adminer**: http://localhost:8080