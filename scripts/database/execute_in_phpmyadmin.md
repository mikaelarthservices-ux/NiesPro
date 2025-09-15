# 🚀 Exécution des Bases de Données via phpMyAdmin

## 📋 **Instructions WAMP/phpMyAdmin**

### 1. **Ouvrir phpMyAdmin**
- Démarrer WAMP 
- Aller sur : http://localhost/phpmyadmin/
- Se connecter avec :
  - **Utilisateur** : `root`
  - **Mot de passe** : *(vide par défaut)*

### 2. **Exécuter le Script SQL**
1. Cliquer sur l'onglet **"SQL"** en haut
2. Copier tout le contenu du fichier `create_databases_wamp.sql`
3. Coller dans la zone de texte
4. Cliquer sur **"Exécuter"**

### 3. **Vérification**
Après exécution, vous devriez voir dans la liste de gauche :
- ✅ `NiesPro_Auth` 
- ✅ `NiesPro_Product`
- ✅ `NiesPro_Stock`
- ✅ `NiesPro_Order`
- ✅ `NiesPro_Payment`
- ✅ `NiesPro_Customer`
- ✅ `NiesPro_Restaurant`
- ✅ `NiesPro_Log`

## 🔗 **Script à Copier/Coller**

Voici le contenu exact à copier dans phpMyAdmin :

```sql
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

-- Message de fin
SELECT 'Toutes les bases NiesPro ont été créées avec succès!' AS Message;
```

## ✅ **Prochaines Étapes**

Une fois les bases créées :
1. **Migration Entity Framework** pour Auth
2. **Tester la connexion** depuis l'API Auth
3. **Développer les autres microservices**