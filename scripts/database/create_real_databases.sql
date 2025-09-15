-- =======================================================================
-- NIESPRO ERP - SCRIPT DE CRÉATION DES BASES DE DONNÉES RÉELLES
-- Version: 1.0.0 Production
-- Date: 2025-09-12
-- =======================================================================

-- Affichage du début du script
SELECT 'Début de création des bases NiesPro...' AS Status;

-- =======================================================================
-- 1. CRÉATION DES BASES DE DONNÉES
-- =======================================================================

-- Base d'authentification (prioritaire)
DROP DATABASE IF EXISTS NiesPro_Auth;
CREATE DATABASE NiesPro_Auth
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des produits
DROP DATABASE IF EXISTS NiesPro_Product;
CREATE DATABASE NiesPro_Product
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des stocks
DROP DATABASE IF EXISTS NiesPro_Stock;
CREATE DATABASE NiesPro_Stock
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des commandes
DROP DATABASE IF EXISTS NiesPro_Order;
CREATE DATABASE NiesPro_Order
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des paiements
DROP DATABASE IF EXISTS NiesPro_Payment;
CREATE DATABASE NiesPro_Payment
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des clients
DROP DATABASE IF EXISTS NiesPro_Customer;
CREATE DATABASE NiesPro_Customer
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base spécifique restaurant
DROP DATABASE IF EXISTS NiesPro_Restaurant;
CREATE DATABASE NiesPro_Restaurant
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des logs et audit
DROP DATABASE IF EXISTS NiesPro_Log;
CREATE DATABASE NiesPro_Log
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- =======================================================================
-- 2. CRÉATION DES UTILISATEURS
-- =======================================================================

-- Supprimer les utilisateurs existants
DROP USER IF EXISTS 'niespro'@'localhost';
DROP USER IF EXISTS 'niespro_reader'@'localhost';
DROP USER IF EXISTS 'niespro_service'@'localhost';

-- Utilisateur principal pour l'application
CREATE USER 'niespro'@'localhost' IDENTIFIED BY 'NiesPro2025!';

-- Utilisateur pour les rapports (lecture seule)
CREATE USER 'niespro_reader'@'localhost' IDENTIFIED BY 'NiesPro2025!Reader';

-- Utilisateur pour les services (backup, maintenance)
CREATE USER 'niespro_service'@'localhost' IDENTIFIED BY 'NiesPro2025!Service';

-- =======================================================================
-- 3. ATTRIBUTION DES PERMISSIONS
-- =======================================================================

-- Permissions complètes pour l'utilisateur principal
GRANT ALL PRIVILEGES ON NiesPro_Auth.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Product.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Stock.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Order.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Payment.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Customer.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Restaurant.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Log.* TO 'niespro'@'localhost';

-- Permissions lecture seule pour les rapports
GRANT SELECT ON NiesPro_Auth.* TO 'niespro_reader'@'localhost';
GRANT SELECT ON NiesPro_Product.* TO 'niespro_reader'@'localhost';
GRANT SELECT ON NiesPro_Stock.* TO 'niespro_reader'@'localhost';
GRANT SELECT ON NiesPro_Order.* TO 'niespro_reader'@'localhost';
GRANT SELECT ON NiesPro_Payment.* TO 'niespro_reader'@'localhost';
GRANT SELECT ON NiesPro_Customer.* TO 'niespro_reader'@'localhost';
GRANT SELECT ON NiesPro_Restaurant.* TO 'niespro_reader'@'localhost';
GRANT SELECT ON NiesPro_Log.* TO 'niespro_reader'@'localhost';

-- Permissions spéciales pour les services
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, INDEX, ALTER ON NiesPro_*.* TO 'niespro_service'@'localhost';

-- Appliquer les changements
FLUSH PRIVILEGES;

-- =======================================================================
-- 4. VÉRIFICATIONS
-- =======================================================================

-- Afficher les bases créées
SELECT 'Bases de données créées:' AS Info;
SHOW DATABASES LIKE 'NiesPro_%';

-- Afficher les utilisateurs créés
SELECT 'Utilisateurs créés:' AS Info;
SELECT User, Host, account_locked, password_expired 
FROM mysql.user 
WHERE User LIKE 'niespro%';

-- Test de connexion avec l'utilisateur principal
SELECT 'Test de connexion - utilisateur niespro:' AS Info;

-- =======================================================================
-- 5. CONFIGURATION DE LA BASE AUTH (PRIORITAIRE)
-- =======================================================================

USE NiesPro_Auth;

-- Affichage de la base active
SELECT DATABASE() AS 'Base Active', NOW() AS 'Timestamp';

-- Créer un utilisateur admin par défaut (sera remplacé par Entity Framework)
-- Cette table sera recréée par les migrations, c'est juste pour vérifier
CREATE TABLE IF NOT EXISTS temp_verification (
    id INT AUTO_INCREMENT PRIMARY KEY,
    message VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO temp_verification (message) VALUES 
('Base NiesPro_Auth créée avec succès'),
('Prête pour les migrations Entity Framework');

-- Afficher le contenu de vérification
SELECT * FROM temp_verification;

-- =======================================================================
-- 6. INFORMATIONS DE CONNEXION
-- =======================================================================

SELECT '=== INFORMATIONS DE CONNEXION ===' AS Info;
SELECT 'Host: localhost' AS Connection_Info
UNION ALL SELECT 'Port: 3306'
UNION ALL SELECT 'User: niespro'
UNION ALL SELECT 'Password: NiesPro2025!'
UNION ALL SELECT 'Database Auth: NiesPro_Auth'
UNION ALL SELECT 'Status: Prêt pour Entity Framework';

-- Fin du script
SELECT 'Script terminé avec succès!' AS Status, NOW() AS Timestamp;