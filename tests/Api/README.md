# 🌐 Collections de Tests API - NiesPro

## 📋 Vue d'ensemble

Ce dossier contient des collections de tests API pour valider le bon fonctionnement des endpoints de chaque microservice.

## 🔧 Services et Endpoints

### 🔐 Auth Service (Port 5001)

#### Endpoints de base
- `GET /health` - Vérification de santé
- `GET /api/v1/auth/info` - Informations du service

#### Endpoints d'authentification
- `POST /api/v1/auth/login` - Connexion utilisateur
- `POST /api/v1/auth/register` - Inscription utilisateur
- `POST /api/v1/auth/refresh` - Renouvellement de token
- `POST /api/v1/auth/logout` - Déconnexion

#### Endpoints de gestion
- `GET /api/v1/users/profile` - Profil utilisateur
- `PUT /api/v1/users/profile` - Mise à jour profil
- `GET /api/v1/users/devices` - Appareils de l'utilisateur

### 💳 Payment Service (Port 5002)

#### Endpoints de base
- `GET /health` - Vérification de santé
- `GET /api/v1/payment/info` - Informations du service

#### Endpoints de paiement
- `POST /api/v1/payments` - Créer un paiement
- `GET /api/v1/payments/{id}` - Détails d'un paiement
- `POST /api/v1/payments/{id}/capture` - Capturer un paiement
- `POST /api/v1/payments/{id}/refund` - Rembourser un paiement

#### Endpoints de méthodes de paiement
- `GET /api/v1/payment-methods` - Liste des méthodes
- `POST /api/v1/payment-methods` - Ajouter une méthode
- `PUT /api/v1/payment-methods/{id}` - Modifier une méthode

### 📦 Order Service (Port 5003)

#### Endpoints de base
- `GET /health` - Vérification de santé
- `GET /api/v1/order/info` - Informations du service

#### Endpoints de commandes
- `POST /api/v1/orders` - Créer une commande
- `GET /api/v1/orders/{id}` - Détails d'une commande
- `PUT /api/v1/orders/{id}/status` - Mettre à jour le statut
- `GET /api/v1/orders/customer/{customerId}` - Commandes d'un client

### 📋 Catalog Service (Port 5004)

#### Endpoints de base
- `GET /health` - Vérification de santé
- `GET /api/v1/catalog/info` - Informations du service

#### Endpoints de produits
- `GET /api/v1/products` - Liste des produits
- `GET /api/v1/products/{id}` - Détails d'un produit
- `POST /api/v1/products` - Créer un produit
- `PUT /api/v1/products/{id}` - Modifier un produit

## 🧪 Scripts de Test

### Test de connectivité
```bash
# Tester tous les endpoints /health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
```

### Test d'authentification complet
```bash
# 1. Inscription
curl -X POST http://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!"
  }'

# 2. Connexion
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

### Test de paiement
```bash
# Créer un paiement
curl -X POST http://localhost:5002/api/v1/payments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "amount": 29.99,
    "currency": "EUR",
    "description": "Test payment"
  }'
```

## 📊 Validation des Réponses

### Codes de statut attendus
- `200 OK` - Opération réussie
- `201 Created` - Ressource créée
- `400 Bad Request` - Requête invalide
- `401 Unauthorized` - Non authentifié
- `403 Forbidden` - Non autorisé
- `404 Not Found` - Ressource non trouvée
- `500 Internal Server Error` - Erreur serveur

### Structure des réponses
```json
{
  "success": true,
  "data": {...},
  "message": "Operation completed successfully",
  "timestamp": "2025-09-15T10:30:00Z"
}
```

### Structure des erreurs
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input provided",
    "details": {...}
  },
  "timestamp": "2025-09-15T10:30:00Z"
}
```

## 🔄 Tests d'intégration

### Scénario complet: Commande avec paiement
1. **Authentification** - Connexion utilisateur
2. **Catalogue** - Recherche de produits
3. **Commande** - Création d'une commande
4. **Paiement** - Traitement du paiement
5. **Validation** - Confirmation de la commande

### Variables d'environnement
```
AUTH_BASE_URL=http://localhost:5001
PAYMENT_BASE_URL=http://localhost:5002
ORDER_BASE_URL=http://localhost:5003
CATALOG_BASE_URL=http://localhost:5004
TEST_USER_EMAIL=test@niespro.com
TEST_USER_PASSWORD=Test123!
```

---

**Note**: Ces tests supposent que tous les services sont démarrés et accessibles sur leurs ports respectifs.