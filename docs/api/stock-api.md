# Stock.API - Documentation

## Description
Service de gestion des stocks et inventaire avec support multi-emplacements.

## Configuration
- **Port HTTP**: 5005
- **Port HTTPS**: 5006
- **Base Path**: /src/Services/Stock/Stock.API
- **Database**: NiesPro_Stock

## Endpoints Disponibles

### Stock Management
- `GET /api/stock` - Liste tous les stocks
- `GET /api/stock/{id}` - Détails d'un stock
- `POST /api/stock` - Créer un nouveau stock
- `PUT /api/stock/{id}` - Mettre à jour un stock
- `DELETE /api/stock/{id}` - Supprimer un stock

### Location Management
- `GET /api/stock/locations` - Liste des emplacements
- `POST /api/stock/locations` - Créer un emplacement
- `PUT /api/stock/locations/{id}` - Modifier un emplacement

### Stock Movements
- `GET /api/stock/movements` - Historique des mouvements
- `POST /api/stock/movements` - Enregistrer un mouvement
- `GET /api/stock/movements/{stockId}` - Mouvements pour un stock

### Suppliers
- `GET /api/suppliers` - Liste des fournisseurs
- `POST /api/suppliers` - Créer un fournisseur
- `PUT /api/suppliers/{id}` - Modifier un fournisseur

### Health Check
- `GET /health` - Status du service

## Status Actuel: ✅ OPÉRATIONNEL

### Tests Validés
- **HTTP Health**: ✅ http://localhost:5005/health
- **HTTPS Health**: ✅ https://localhost:5006/health
- **Response**: `{"status":"Healthy","service":"Stock.API"}`
- **Response Time**: < 50ms

## Modèles de Données

### Stock
```json
{
  "id": "uuid",
  "productId": "uuid",
  "locationId": "uuid",
  "quantity": 100,
  "reservedQuantity": 10,
  "minimumLevel": 20,
  "maximumLevel": 1000,
  "lastUpdated": "2024-09-22T16:00:00Z"
}
```

### Location
```json
{
  "id": "uuid",
  "name": "Entrepôt Principal",
  "address": "123 Rue du Commerce",
  "type": "WAREHOUSE",
  "capacity": 10000,
  "isActive": true
}
```

### StockMovement
```json
{
  "id": "uuid",
  "stockId": "uuid",
  "type": "IN|OUT|TRANSFER",
  "quantity": 50,
  "reason": "Purchase Order #12345",
  "timestamp": "2024-09-22T16:00:00Z",
  "userId": "uuid"
}
```

## Authentication
Requiert un token JWT Bearer dans le header Authorization:
```
Authorization: Bearer <jwt_token>
```

## Exemples d'utilisation

### Vérifier le stock d'un produit
```bash
GET /api/stock?productId=123
Authorization: Bearer <token>
```

### Ajouter du stock
```bash
POST /api/stock/movements
Authorization: Bearer <token>
Content-Type: application/json

{
  "stockId": "uuid",
  "type": "IN",
  "quantity": 100,
  "reason": "Réception commande fournisseur"
}
```

## Codes d'erreur
- **200**: Succès
- **400**: Requête invalide
- **401**: Non authentifié
- **403**: Non autorisé
- **404**: Ressource non trouvée
- **409**: Conflit (stock insuffisant)
- **500**: Erreur serveur

## Swagger UI
- **URL**: http://localhost:5005/swagger
- **Description**: Interface interactive pour tester les APIs

## Démarrage
```bash
cd src/Services/Stock/Stock.API
dotnet run
```

Le service démarre automatiquement sur les ports 5005 (HTTP) et 5006 (HTTPS).