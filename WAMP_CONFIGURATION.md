# 🚀 Configuration NiesPro avec WAMP

## 📋 Étapes avec WAMP Existant

### 1. Démarrer WAMP
- Cliquer sur l'icône WAMP dans la barre des tâches
- S'assurer que l'icône est **verte** (tous les services démarrés)
- Si orange ou rouge, cliquer → "Start All Services"

### 2. Accéder à phpMyAdmin
- Ouvrir : http://localhost/phpmyadmin/
- **Utilisateur** : `root`
- **Mot de passe** : (laisser vide par défaut, ou votre mot de passe WAMP)

### 3. Configuration Spécifique WAMP

#### Informations de connexion WAMP typiques :
- **Host** : localhost
- **Port** : 3306
- **User** : root
- **Password** : (vide par défaut, ou votre mot de passe)

#### Mettre à jour appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=NiesPro_Auth;Uid=root;Pwd=;",
    "Redis": "localhost:6379"
  }
}
```

*Note : Si WAMP a un mot de passe root, remplacez `Pwd=;` par `Pwd=votre_mot_de_passe;`*

## 🗄️ Création des Bases via phpMyAdmin

### Méthode 1 : Interface Graphique
1. Aller dans phpMyAdmin
2. Cliquer sur "Bases de données"
3. Créer une par une :
   - `NiesPro_Auth`
   - `NiesPro_Product` 
   - `NiesPro_Stock`
   - `NiesPro_Order`
   - `NiesPro_Payment`
   - `NiesPro_Customer`
   - `NiesPro_Restaurant`
   - `NiesPro_Log`

### Méthode 2 : Script SQL (Recommandé)
1. Dans phpMyAdmin, cliquer sur "SQL"
2. Copier-coller le script ci-dessous
3. Cliquer "Exécuter"

## 📝 Script SQL pour WAMP

```sql
-- Script de création bases NiesPro pour WAMP
-- Exécuter dans phpMyAdmin > SQL

-- Créer les bases de données
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

-- Créer un utilisateur pour l'application (optionnel)
CREATE USER IF NOT EXISTS 'niespro'@'localhost' IDENTIFIED BY 'NiesPro2025!';
GRANT ALL PRIVILEGES ON NiesPro_*.* TO 'niespro'@'localhost';
FLUSH PRIVILEGES;

-- Vérification
SHOW DATABASES LIKE 'NiesPro_%';
```

## ✅ Vérification

Après exécution du script, vous devriez voir dans phpMyAdmin :
- 8 nouvelles bases de données commençant par "NiesPro_"
- Possibilité de naviguer dans chaque base
- Bases vides (les tables seront créées par Entity Framework)

## 🔧 Test de Connexion

### Script PowerShell pour tester WAMP
```powershell
# Tester la connexion à WAMP MySQL
$connectionString = "Server=localhost;Port=3306;Database=NiesPro_Auth;Uid=root;Pwd=;"

# Test de connectivité
Test-NetConnection -ComputerName localhost -Port 3306

# Si MySQL client est disponible
mysql -h localhost -u root -p -e "SHOW DATABASES LIKE 'NiesPro_%';"
```

## 🚀 Prochaines Étapes

Une fois les bases créées dans WAMP :

1. **Créer les migrations Entity Framework**
2. **Appliquer les migrations** 
3. **Démarrer l'API Auth**
4. **Voir les tables dans phpMyAdmin**

## 💡 Avantages avec WAMP

- ✅ **phpMyAdmin** : Interface web intuitive
- ✅ **Apache** : Serveur web local disponible
- ✅ **PHP** : Possibilité d'ajouter des scripts admin
- ✅ **MySQL** : Base de données robuste
- ✅ **Gestion facile** : Interface WAMP simple

---

**🎯 Action Immédiate** : Démarrez WAMP et ouvrez http://localhost/phpmyadmin/ pour créer les bases !