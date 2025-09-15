# 🗄️ Guide d'Accès à la Base de Données NiesPro

## 📍 Localisation Actuelle

**Votre base de données se trouve dans :**
- 🐳 **Docker Container** : `niespro-mysql`
- 🌐 **Accessible via** : `localhost:3306`
- 📊 **Type** : MySQL 8.0
- 🗂️ **Bases configurées** : 8 bases séparées par microservice

## 🚀 Comment Démarrer et Accéder

### 1. Exécuter le Script de Démarrage
```powershell
# Dans PowerShell (depuis le dossier NiesPro)
.\scripts\start-database.ps1
```

### 2. Accès via Interface Web (Recommandé)
- **URL** : http://localhost:8080
- **Serveur** : `mysql` (nom du conteneur)
- **Utilisateur** : `root`
- **Mot de passe** : `NiesPro2025!Root`
- **Base** : `NiesPro_Auth`

### 3. Accès Direct via Terminal
```bash
# Connexion directe au conteneur MySQL
docker exec -it niespro-mysql mysql -u root -p
# Mot de passe : NiesPro2025!Root

# Puis dans MySQL :
USE NiesPro_Auth;
SHOW TABLES;
```

## 📊 Structure Actuelle de la Base

### 🎯 État des Tables

**Actuellement, les tables sont définies dans :**
1. **Entity Framework** (C# Classes) ✅
2. **Script SQL** d'initialisation ✅
3. **Migrations EF** ⚠️ (à créer)

### 🏗️ Tables Prévues dans NiesPro_Auth

| Table | Description | État |
|-------|-------------|------|
| `Users` | Utilisateurs du système | 📋 Définie |
| `Roles` | Rôles (Admin, Manager, etc.) | 📋 Définie |
| `Permissions` | Permissions granulaires | 📋 Définie |
| `UserRoles` | Association Utilisateur-Rôle | 📋 Définie |
| `RolePermissions` | Association Rôle-Permission | 📋 Définie |
| `Devices` | Appareils autorisés | 📋 Définie |
| `UserSessions` | Sessions actives | 📋 Définie |
| `AuditLogs` | Journal d'audit | 📋 Définie |

## 🔍 Comment Voir la Structure

### Option A : Interface Web Adminer
1. Démarrer : `.\scripts\start-database.ps1`
2. Ouvrir : http://localhost:8080
3. Se connecter avec les identifiants ci-dessus
4. Explorer les bases et tables

### Option B : Générer les Migrations EF
```powershell
# Dans le dossier Auth.Infrastructure
cd src\Services\Auth\Auth.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ..\Auth.API
```

### Option C : Voir le Code des Entités
Les entités sont définies dans :
- `src/Services/Auth/Auth.Domain/Entities/`
- Configuration EF dans `src/Services/Auth/Auth.Infrastructure/Persistence/`

## 🎯 Prochaines Actions

### Pour Voir la Base Immédiatement
1. ✅ Ouvrir PowerShell en tant qu'administrateur
2. ✅ Aller dans le dossier NiesPro
3. ✅ Exécuter : `.\scripts\start-database.ps1`
4. ✅ Ouvrir http://localhost:8080
5. ✅ Se connecter et explorer

### Pour Créer les Tables Définitives
1. Créer les migrations Entity Framework
2. Appliquer les migrations
3. Insérer les données de test
4. Tester l'API Auth

## 🛠️ Résolution de Problèmes

### Docker ne démarre pas
- Vérifier que Docker Desktop est lancé
- Relancer Docker Desktop en tant qu'administrateur

### Port 3306 occupé
```powershell
# Vérifier qui utilise le port
netstat -ano | findstr :3306
# Arrêter MySQL local si nécessaire
```

### Connexion refusée
- Attendre 30 secondes après le démarrage
- Vérifier les logs : `docker logs niespro-mysql`

## 📱 Interfaces Disponibles

| Service | URL | Identifiants |
|---------|-----|--------------|
| 🔧 **Adminer** | http://localhost:8080 | root / NiesPro2025!Root |
| 🗄️ **MySQL Direct** | localhost:3306 | root / NiesPro2025!Root |
| 🔴 **Redis** | localhost:6379 | (pas de mot de passe) |
| 🐰 **RabbitMQ** | http://localhost:15672 | niespro / NiesPro2025!Rabbit |

---

**💡 Conseil** : Commencez par exécuter le script `start-database.ps1` puis ouvrez Adminer pour voir votre base de données !