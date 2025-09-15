-- =======================================================================
-- NIESPRO ERP - SCRIPT POUR WAMP/phpMyAdmin
-- Version: 1.0.0 WAMP
-- Date: 2025-09-12
-- =======================================================================

-- Message de début
SELECT 'Création des bases NiesPro avec WAMP...' AS Message;

-- =======================================================================
-- CRÉATION DES BASES DE DONNÉES
-- =======================================================================

-- Base d'authentification (prioritaire)
CREATE DATABASE IF NOT EXISTS NiesPro_Auth
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des produits  
CREATE DATABASE IF NOT EXISTS NiesPro_Product
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des stocks
CREATE DATABASE IF NOT EXISTS NiesPro_Stock
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des commandes
CREATE DATABASE IF NOT EXISTS NiesPro_Order
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des paiements
CREATE DATABASE IF NOT EXISTS NiesPro_Payment
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des clients
CREATE DATABASE IF NOT EXISTS NiesPro_Customer
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base spécifique restaurant
CREATE DATABASE IF NOT EXISTS NiesPro_Restaurant
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Base des logs et audit
CREATE DATABASE IF NOT EXISTS NiesPro_Log
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- =======================================================================
-- CRÉATION D'UN UTILISATEUR DÉDIÉ (OPTIONNEL)
-- =======================================================================

-- Créer un utilisateur spécifique pour NiesPro (recommandé)
CREATE USER IF NOT EXISTS 'niespro'@'localhost' IDENTIFIED BY 'NiesPro2025!';

-- Accorder tous les privilèges sur les bases NiesPro
GRANT ALL PRIVILEGES ON NiesPro_Auth.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Product.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Stock.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Order.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Payment.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Customer.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Restaurant.* TO 'niespro'@'localhost';
GRANT ALL PRIVILEGES ON NiesPro_Log.* TO 'niespro'@'localhost';

-- Appliquer les changements
FLUSH PRIVILEGES;

-- =======================================================================
-- VÉRIFICATIONS
-- =======================================================================

-- Lister toutes les bases NiesPro créées
SELECT 'Bases de données NiesPro créées:' AS Info;
SHOW DATABASES LIKE 'NiesPro_%';

-- Vérifier l'utilisateur créé
SELECT 'Utilisateur NiesPro créé:' AS Info;
SELECT User, Host FROM mysql.user WHERE User = 'niespro';

-- =======================================================================
-- TEST DE LA BASE AUTH
-- =======================================================================

-- Utiliser la base Auth pour les tests
USE NiesPro_Auth;

-- Créer une table temporaire pour vérifier le fonctionnement
CREATE TABLE IF NOT EXISTS test_connection (
    id INT AUTO_INCREMENT PRIMARY KEY,
    message VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insérer une donnée de test
INSERT INTO test_connection (message) VALUES 
('Base NiesPro_Auth opérationnelle via WAMP'),
('Prête pour Entity Framework migrations');

-- Afficher les données de test
SELECT * FROM test_connection;

-- =======================================================================
-- INFORMATIONS DE CONNEXION
-- =======================================================================

SELECT '=== CONFIGURATION POUR NIESPRO ===' AS Info;

SELECT 'WAMP Configuration' AS Setting, 'Valeur' AS Value
UNION ALL SELECT 'Host', 'localhost'
UNION ALL SELECT 'Port', '3306'
UNION ALL SELECT 'Database', 'NiesPro_Auth'
UNION ALL SELECT 'Username Option 1', 'root'
UNION ALL SELECT 'Password Option 1', '(votre mot de passe WAMP)'
UNION ALL SELECT 'Username Option 2', 'niespro'
UNION ALL SELECT 'Password Option 2', 'NiesPro2025!'
UNION ALL SELECT 'phpMyAdmin URL', 'http://localhost/phpmyadmin/'
UNION ALL SELECT 'Status', 'Prêt pour Entity Framework';

-- Message de fin
SELECT 'Script terminé avec succès! Vous pouvez maintenant utiliser Entity Framework.' AS Message;