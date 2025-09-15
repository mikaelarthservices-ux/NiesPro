# 🗄️ Création de Base de Données Sans Docker

## 📍 Situation Actuelle
Vous ne voyez aucune base de données car Docker n'est pas démarré ou accessible.

## 🎯 Solutions Alternatives

### Option A : Installer MySQL Localement (Recommandé)

#### 1. Télécharger MySQL
- Aller sur https://dev.mysql.com/downloads/mysql/
- Télécharger MySQL Community Server 8.0
- Installer avec ces paramètres :
  - Port : 3306
  - Root password : `NiesPro2025!Root`

#### 2. Créer les Bases Manuellement
```sql
-- Se connecter à MySQL et exécuter :
CREATE DATABASE NiesPro_Auth CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE NiesPro_Product CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE NiesPro_Stock CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE NiesPro_Order CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE NiesPro_Payment CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE NiesPro_Customer CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE NiesPro_Restaurant CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE NiesPro_Log CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Créer un utilisateur pour l'application
CREATE USER 'niespro'@'localhost' IDENTIFIED BY 'NiesPro2025!';
GRANT ALL PRIVILEGES ON NiesPro_*.* TO 'niespro'@'localhost';
FLUSH PRIVILEGES;
```

### Option B : Utiliser SQLite (Plus Simple)

#### 1. Modifier la Configuration
Dans `Auth.API/appsettings.json` :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NiesPro.db"
  }
}
```

#### 2. Installer le Package SQLite
```bash
cd src/Services/Auth/Auth.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

### Option C : Forcer le Démarrage Docker

#### 1. Démarrer Docker Desktop
- Ouvrir Docker Desktop en tant qu'administrateur
- Attendre qu'il soit complètement démarré (icône verte)

#### 2. Redémarrer PowerShell en Administrateur
- Clic droit sur PowerShell → "Exécuter en tant qu'administrateur"

#### 3. Essayer à nouveau
```powershell
cd C:\Users\HP\Documents\projets\NiesPro
docker-compose up -d mysql
```

## 🎯 Solution Immédiate : SQLite

Créons une version SQLite pour que vous puissiez voir la base immédiatement :