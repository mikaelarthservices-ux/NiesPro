# Documentation des Routes Auth.API

## Vue d'ensemble du Service

**Auth.API** - Service d'Authentification NiesPro
- **URL Base**: `https://localhost:5011` (HTTPS) / `http://localhost:5001` (HTTP)
- **Version**: .NET 8
- **Base de données**: MySQL
- **Authentification**: JWT Bearer Token
- **Documentation Swagger**: `https://localhost:5011/swagger`

---

## Routes Disponibles

### 1. Health Check

**Vérification de l'état du service**

- **Méthode**: `GET`
- **Endpoint**: `/health`
- **URL Complète**: `https://localhost:5011/health` (HTTPS) ou `http://localhost:5001/health` (HTTP)
- **Authentification**: Aucune
- **Description**: Vérifie si le service est opérationnel

#### Réponses
- **200 OK**: `"Healthy"`
- **500 Internal Server Error**: Service indisponible

#### Exemple d'utilisation
```bash
curl -k https://localhost:5001/health
```

---

### 2. Inscription Utilisateur

**Création d'un nouveau compte utilisateur**

- **Méthode**: `POST`
- **Endpoint**: `/api/v1/auth/register`
- **URL Complète**: `https://localhost:5011/api/v1/auth/register` (HTTPS) ou `http://localhost:5001/api/v1/auth/register` (HTTP)
- **Authentification**: Aucune
- **Content-Type**: `application/json`

#### Body de la requête
```json
{
  "username": "string (requis, unique)",
  "email": "string (requis, unique, format email valide)",
  "password": "string (requis, minimum 8 caractères)",
  "confirmPassword": "string (requis, doit correspondre au password)"
}
```

#### Validations
- **Username**: 
  - Requis
  - Doit être unique
  - Caractères alphanumériques et underscore autorisés
- **Email**: 
  - Requis
  - Format email valide
  - Doit être unique
- **Password**: 
  - Requis
  - Minimum 8 caractères
  - Doit contenir au moins une majuscule, une minuscule et un chiffre
- **ConfirmPassword**: 
  - Requis
  - Doit être identique au password

#### Réponses
**201 Created** - Inscription réussie
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_string",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "uuid",
    "username": "username",
    "email": "email@example.com"
  }
}
```

**400 Bad Request** - Erreurs de validation
```json
{
  "errors": {
    "Username": ["Username is already taken"],
    "Email": ["Email is already registered"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

**409 Conflict** - Utilisateur existe déjà

#### Exemple d'utilisation
```bash
curl -k -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123"
  }'
```

---

### 3. Connexion Utilisateur

**Authentification d'un utilisateur existant**

- **Méthode**: `POST`
- **Endpoint**: `/api/v1/auth/login`
- **URL Complète**: `https://localhost:5011/api/v1/auth/login` (HTTPS) ou `http://localhost:5001/api/v1/auth/login` (HTTP)
- **Authentification**: Aucune
- **Content-Type**: `application/json`

#### Body de la requête
```json
{
  "username": "string (requis)",
  "password": "string (requis)"
}
```

#### Réponses
**200 OK** - Connexion réussie
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_string",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "uuid",
    "username": "username",
    "email": "email@example.com"
  }
}
```

**401 Unauthorized** - Identifiants invalides
```json
{
  "message": "Invalid username or password"
}
```

**400 Bad Request** - Données manquantes

#### Exemple d'utilisation
```bash
curl -k -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "SecurePass123"
  }'
```

---

### 4. Déconnexion Utilisateur

**Déconnexion et invalidation du token**

- **Méthode**: `POST`
- **Endpoint**: `/api/v1/auth/logout`
- **URL Complète**: `https://localhost:5011/api/v1/auth/logout` (HTTPS) ou `http://localhost:5001/api/v1/auth/logout` (HTTP)
- **Authentification**: **Bearer Token requis**
- **Header requis**: `Authorization: Bearer <votre_access_token>`

#### Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Body
Aucun body requis

#### Réponses
**200 OK** - Déconnexion réussie
```json
{
  "message": "Successfully logged out",
  "success": true
}
```

**401 Unauthorized** - Token invalide ou manquant
```json
{
  "message": "Token is required"
}
```

#### Exemple d'utilisation
```bash
curl -k -X POST https://localhost:5001/api/v1/auth/logout \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

---

## Sécurité et Authentification JWT

### Format du Token JWT

Les tokens JWT générés contiennent les claims suivants :
- **sub** (Subject): ID de l'utilisateur
- **username**: Nom d'utilisateur
- **email**: Email de l'utilisateur
- **iat** (Issued At): Timestamp de création
- **exp** (Expiration): Timestamp d'expiration (1 heure par défaut)
- **jti** (JWT ID): Identifiant unique du token

### Utilisation du Token

Pour les routes nécessitant une authentification, incluez le token dans l'en-tête :
```
Authorization: Bearer <access_token>
```

### Durée de vie des Tokens

- **Access Token**: 1 heure (3600 secondes)
- **Refresh Token**: 7 jours (configurable)

---

## Codes d'Erreur Courants

| Code | Status | Description |
|------|--------|-------------|
| 200 | OK | Requête réussie |
| 201 | Created | Ressource créée avec succès |
| 400 | Bad Request | Données invalides ou manquantes |
| 401 | Unauthorized | Authentification requise ou token invalide |
| 409 | Conflict | Conflit (ex: utilisateur existe déjà) |
| 500 | Internal Server Error | Erreur serveur |

---

## Exemples de Scénarios d'Usage

### Scénario 1: Inscription complète
```bash
# 1. Vérifier que le service est disponible
curl -k https://localhost:5001/health

# 2. S'inscrire
curl -k -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "nouveauuser",
    "email": "nouveau@example.com",
    "password": "MonMotDePasse123",
    "confirmPassword": "MonMotDePasse123"
  }'

# Réponse: tokens JWT + informations utilisateur
```

### Scénario 2: Connexion et utilisation
```bash
# 1. Se connecter
curl -k -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "nouveauuser",
    "password": "MonMotDePasse123"
  }'

# 2. Utiliser le token pour se déconnecter
curl -k -X POST https://localhost:5001/api/v1/auth/logout \
  -H "Authorization: Bearer VOTRE_TOKEN_ICI"
```

---

## Bonnes Pratiques de Sécurité

### Pour les Développeurs
- ✅ Toujours utiliser HTTPS en production
- ✅ Stocker les tokens de manière sécurisée (pas dans localStorage)
- ✅ Implémenter la rotation des refresh tokens
- ✅ Valider et assainir toutes les entrées utilisateur
- ✅ Configurer CORS appropriément pour votre domaine
- ✅ Implémenter un rate limiting sur les routes d'authentification
- ✅ Logger les tentatives d'authentification pour la sécurité

### Pour les Utilisateurs de l'API
- ✅ Ne jamais exposer les tokens dans les URLs
- ✅ Gérer l'expiration des tokens gracieusement
- ✅ Implémenter un mécanisme de retry avec exponential backoff
- ✅ Nettoyer les tokens lors de la déconnexion

---

## Configuration Requise

### Variables d'Environnement
- `JWT_SECRET`: Clé secrète pour signer les JWT
- `JWT_EXPIRES_IN`: Durée de vie des access tokens (défaut: 1h)
- `REFRESH_TOKEN_EXPIRES_IN`: Durée de vie des refresh tokens (défaut: 7j)
- `DATABASE_CONNECTION_STRING`: Chaîne de connexion MySQL

### Dépendances
- Base de données MySQL configurée
- Tables utilisateurs créées via migrations EF Core
- Configuration CORS pour les clients web

---

*Documentation générée le: $(Get-Date)*
*Version API: 1.0.0*
*Service: Auth.API - NiesPro ERP*