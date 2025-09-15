# 🗄️ Installation MySQL Server Réel sur Windows

## 📋 Étapes d'Installation

### 1. Télécharger MySQL Server
- Aller sur : https://dev.mysql.com/downloads/mysql/
- Choisir "MySQL Community Server 8.0.x"
- Sélectionner "Windows (x86, 64-bit), MSI Installer"
- Télécharger le fichier complet (mysql-installer-community-8.0.x.x.msi)

### 2. Installation
1. **Exécuter l'installeur en tant qu'administrateur**
2. **Choisir "Developer Default"** ou "Server only"
3. **Configuration** :
   - Port : `3306` (par défaut)
   - Root Password : `NiesPro2025!Root`
   - Créer un utilisateur : `niespro` / `NiesPro2025!`

### 3. Démarrer MySQL Service
```cmd
# Démarrer le service MySQL
net start MySQL80

# Vérifier que MySQL fonctionne
mysql -u root -p
# Entrer le password : NiesPro2025!Root
```

## 🗄️ Script de Création des Bases

Une fois MySQL installé, exécuter ce script SQL :

```sql
-- =======================================================================
-- SCRIPT DE CRÉATION DES BASES NIESPRO - PRODUCTION
-- =======================================================================

-- 1. Créer toutes les bases de données
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

-- 2. Créer les utilisateurs
CREATE USER IF NOT EXISTS 'niespro'@'localhost' IDENTIFIED BY 'NiesPro2025!';
CREATE USER IF NOT EXISTS 'niespro_reader'@'localhost' IDENTIFIED BY 'NiesPro2025!Reader';

-- 3. Accorder les permissions
GRANT ALL PRIVILEGES ON NiesPro_*.* TO 'niespro'@'localhost';
GRANT SELECT ON NiesPro_*.* TO 'niespro_reader'@'localhost';
FLUSH PRIVILEGES;

-- 4. Vérifier les bases créées
SHOW DATABASES;

-- 5. Afficher les utilisateurs
SELECT User, Host FROM mysql.user WHERE User LIKE 'niespro%';
```

## 🔧 Configuration de l'Application

### Mettre à jour appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=NiesPro_Auth;Uid=niespro;Pwd=NiesPro2025!;",
    "Redis": "localhost:6379"
  }
}
```

## 🚀 Commandes de Vérification

### Tester la connexion
```cmd
mysql -h localhost -P 3306 -u niespro -p
# Password: NiesPro2025!

# Une fois connecté :
USE NiesPro_Auth;
SHOW TABLES;
```

### Créer les tables avec Entity Framework
```powershell
cd C:\Users\HP\Documents\projets\NiesPro\src\Services\Auth\Auth.Infrastructure

# Créer la migration
dotnet ef migrations add InitialCreate --startup-project ../Auth.API

# Appliquer la migration
dotnet ef database update --startup-project ../Auth.API
```

## 📊 Interface d'Administration

### Option 1: MySQL Workbench (Recommandé)
- Télécharger : https://dev.mysql.com/downloads/workbench/
- Connexion : localhost:3306, user: niespro

### Option 2: phpMyAdmin (Web)
- Installer XAMPP ou WAMP
- Utiliser phpMyAdmin via http://localhost/phpmyadmin

### Option 3: DBeaver (Gratuit)
- Télécharger : https://dbeaver.io/download/
- Client universel pour bases de données

## ✅ Validation Finale

Une fois tout installé, vous devriez voir :
1. ✅ MySQL Service actif
2. ✅ 8 bases de données NiesPro créées
3. ✅ Utilisateurs niespro configurés
4. ✅ Connexion possible via MySQL Workbench
5. ✅ API Auth qui peut se connecter à la base

## 🎯 Prochaines Étapes

1. **Installer MySQL Server**
2. **Exécuter le script de création**
3. **Tester la connexion**
4. **Créer les migrations Entity Framework**
5. **Voir les tables réelles dans MySQL Workbench**