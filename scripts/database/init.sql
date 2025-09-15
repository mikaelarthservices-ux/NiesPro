-- =======================================================================
-- NiesPro ERP - Script d'initialisation de base de données
-- Version: 1.0.0
-- Date: 2025-09-12
-- =======================================================================

-- Création de la base de données principale
CREATE DATABASE IF NOT EXISTS NiesPro_Auth
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS NiesPro_Product
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS NiesPro_Stock
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS NiesPro_Order
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS NiesPro_Payment
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS NiesPro_Customer
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS NiesPro_Restaurant
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS NiesPro_Log
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- =======================================================================
-- Utilisateurs et permissions
-- =======================================================================

-- Utilisateur pour les services (lecture/écriture)
CREATE USER IF NOT EXISTS 'niespro_service'@'%' IDENTIFIED BY 'NiesPro2025!Service';

-- Utilisateur pour les rapports (lecture seule)
CREATE USER IF NOT EXISTS 'niespro_reader'@'%' IDENTIFIED BY 'NiesPro2025!Reader';

-- Permissions pour l'utilisateur service (accès complet)
GRANT ALL PRIVILEGES ON NiesPro_Auth.* TO 'niespro_service'@'%';
GRANT ALL PRIVILEGES ON NiesPro_Product.* TO 'niespro_service'@'%';
GRANT ALL PRIVILEGES ON NiesPro_Stock.* TO 'niespro_service'@'%';
GRANT ALL PRIVILEGES ON NiesPro_Order.* TO 'niespro_service'@'%';
GRANT ALL PRIVILEGES ON NiesPro_Payment.* TO 'niespro_service'@'%';
GRANT ALL PRIVILEGES ON NiesPro_Customer.* TO 'niespro_service'@'%';
GRANT ALL PRIVILEGES ON NiesPro_Restaurant.* TO 'niespro_service'@'%';
GRANT ALL PRIVILEGES ON NiesPro_Log.* TO 'niespro_service'@'%';

-- Permissions pour l'utilisateur lecture seule
GRANT SELECT ON NiesPro_Auth.* TO 'niespro_reader'@'%';
GRANT SELECT ON NiesPro_Product.* TO 'niespro_reader'@'%';
GRANT SELECT ON NiesPro_Stock.* TO 'niespro_reader'@'%';
GRANT SELECT ON NiesPro_Order.* TO 'niespro_reader'@'%';
GRANT SELECT ON NiesPro_Payment.* TO 'niespro_reader'@'%';
GRANT SELECT ON NiesPro_Customer.* TO 'niespro_reader'@'%';
GRANT SELECT ON NiesPro_Restaurant.* TO 'niespro_reader'@'%';
GRANT SELECT ON NiesPro_Log.* TO 'niespro_reader'@'%';

FLUSH PRIVILEGES;

-- =======================================================================
-- Tables du service Auth (base pour démarrer)
-- =======================================================================

USE NiesPro_Auth;

-- Table des utilisateurs
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    IsActive BOOLEAN DEFAULT TRUE,
    EmailConfirmed BOOLEAN DEFAULT FALSE,
    PhoneNumber VARCHAR(20),
    LastLoginAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy VARCHAR(50),
    UpdatedBy VARCHAR(50),
    IsDeleted BOOLEAN DEFAULT FALSE,
    DeletedAt DATETIME,
    DeletedBy VARCHAR(50),
    
    INDEX idx_username (Username),
    INDEX idx_email (Email),
    INDEX idx_isactive (IsActive),
    INDEX idx_isdeleted (IsDeleted)
);

-- Table des rôles
CREATE TABLE Roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy VARCHAR(50),
    UpdatedBy VARCHAR(50),
    
    INDEX idx_name (Name),
    INDEX idx_isactive (IsActive)
);

-- Table des permissions
CREATE TABLE Permissions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description TEXT,
    Module VARCHAR(50) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_name (Name),
    INDEX idx_module (Module),
    INDEX idx_isactive (IsActive)
);

-- Table de liaison utilisateurs-rôles
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    AssignedBy VARCHAR(50),
    
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

-- Table de liaison rôles-permissions
CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    AssignedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    AssignedBy VARCHAR(50),
    
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);

-- Table des appareils autorisés
CREATE TABLE Devices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DeviceKey VARCHAR(255) NOT NULL UNIQUE,
    DeviceName VARCHAR(100) NOT NULL,
    DeviceType ENUM('desktop', 'mobile', 'tablet', 'web') DEFAULT 'desktop',
    UserId INT,
    IsActive BOOLEAN DEFAULT TRUE,
    LastUsedAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_devicekey (DeviceKey),
    INDEX idx_userid (UserId),
    INDEX idx_isactive (IsActive)
);

-- Table des sessions utilisateur
CREATE TABLE UserSessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    DeviceId INT,
    AccessToken VARCHAR(1000),
    RefreshToken VARCHAR(500),
    ExpiresAt DATETIME NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE SET NULL,
    INDEX idx_userid (UserId),
    INDEX idx_deviceid (DeviceId),
    INDEX idx_refreshtoken (RefreshToken),
    INDEX idx_expiresat (ExpiresAt),
    INDEX idx_isactive (IsActive)
);

-- Table d'audit des actions
CREATE TABLE AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    DeviceId INT,
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(100),
    EntityId VARCHAR(50),
    OldValues JSON,
    NewValues JSON,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE SET NULL,
    INDEX idx_userid (UserId),
    INDEX idx_action (Action),
    INDEX idx_entitytype (EntityType),
    INDEX idx_timestamp (Timestamp)
);

-- =======================================================================
-- Données de base pour démarrer
-- =======================================================================

-- Insertion des rôles de base
INSERT INTO Roles (Name, Description, CreatedBy) VALUES
('admin', 'Administrateur système avec tous les droits', 'system'),
('manager', 'Manager avec droits de gestion', 'system'),
('cashier', 'Caissier pour les ventes', 'system'),
('server', 'Serveur pour le restaurant', 'system'),
('stock_manager', 'Gestionnaire de stock', 'system');

-- Insertion des permissions de base
INSERT INTO Permissions (Name, Description, Module) VALUES
-- Auth permissions
('auth.read', 'Consulter les utilisateurs', 'auth'),
('auth.write', 'Créer/modifier les utilisateurs', 'auth'),
('auth.admin', 'Administration complète des utilisateurs', 'auth'),

-- Product permissions
('product.read', 'Consulter les produits', 'product'),
('product.write', 'Créer/modifier les produits', 'product'),
('product.delete', 'Supprimer les produits', 'product'),

-- Order permissions
('order.read', 'Consulter les commandes', 'order'),
('order.write', 'Créer/modifier les commandes', 'order'),
('order.cancel', 'Annuler les commandes', 'order'),

-- Stock permissions
('stock.read', 'Consulter les stocks', 'stock'),
('stock.write', 'Gérer les mouvements de stock', 'stock'),
('stock.inventory', 'Faire les inventaires', 'stock'),

-- Customer permissions
('customer.read', 'Consulter les clients', 'customer'),
('customer.write', 'Créer/modifier les clients', 'customer'),

-- Report permissions
('report.read', 'Consulter les rapports', 'report'),
('report.export', 'Exporter les rapports', 'report'),

-- Admin permissions
('admin.users', 'Gestion des utilisateurs', 'admin'),
('admin.settings', 'Paramètres système', 'admin'),
('admin.logs', 'Consulter les logs', 'admin');

-- Attribution des permissions aux rôles
-- Admin (tous les droits)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy)
SELECT 1, Id, 'system' FROM Permissions;

-- Manager (tous sauf admin)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy)
SELECT 2, Id, 'system' FROM Permissions WHERE Module != 'admin';

-- Caissier (lecture produits/clients, gestion commandes/paiements)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy)
SELECT 3, Id, 'system' FROM Permissions 
WHERE Name IN ('product.read', 'customer.read', 'customer.write', 'order.read', 'order.write', 'order.cancel');

-- Serveur restaurant (lecture produits, gestion commandes restaurant)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy)
SELECT 4, Id, 'system' FROM Permissions 
WHERE Name IN ('product.read', 'customer.read', 'order.read', 'order.write');

-- Gestionnaire stock (gestion stock et produits)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy)
SELECT 5, Id, 'system' FROM Permissions 
WHERE Name IN ('product.read', 'product.write', 'stock.read', 'stock.write', 'stock.inventory', 'report.read');

-- Création de l'utilisateur admin par défaut
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsActive, EmailConfirmed, CreatedBy) VALUES
('admin', 'admin@niespro.com', '$2a$11$JKTByGtrZARM9i0uHWKjzumgz6AJFl0zJ2roEU9xg.Q6KxN4Y3c2G', 'Admin', 'System', TRUE, TRUE, 'system');
-- Mot de passe par défaut: Admin123!

-- Attribution du rôle admin à l'utilisateur admin
INSERT INTO UserRoles (UserId, RoleId, AssignedBy) VALUES (1, 1, 'system');

-- =======================================================================
-- Fin du script d'initialisation
-- =======================================================================

SELECT 'Base de données NiesPro initialisée avec succès!' AS Status;