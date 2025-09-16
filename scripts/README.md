# Scripts de Lancement NiesPro

Ce dossier contient les scripts batch pour lancer l'architecture microservices NiesPro de manière professionnelle.

## Scripts Disponibles

### `start-all-services.bat`
Lance tous les microservices dans des terminaux externes séparés :
- Auth.API sur le port 5001
- Order.API sur le port 5002  
- Catalog.API sur le port 5003
- Gateway.API sur le port 5000

### Scripts Individuels
- `start-auth-api.bat` - Lance uniquement Auth.API
- `start-order-api.bat` - Lance uniquement Order.API
- `start-catalog-api.bat` - Lance uniquement Catalog.API
- `start-gateway-api.bat` - Lance uniquement Gateway.API

### `stop-all-services.bat`
Arrête tous les services dotnet en cours d'exécution.

## Usage Professionnel

1. **Lancement complet** :
   ```
   start-all-services.bat
   ```

2. **Lancement individuel** :
   ```
   start-auth-api.bat
   start-order-api.bat
   start-catalog-api.bat
   start-gateway-api.bat
   ```

3. **Arrêt complet** :
   ```
   stop-all-services.bat
   ```

## URLs d'Accès

- **Gateway API** : https://localhost:5000
  - Documentation Swagger : https://localhost:5000/swagger
  - Health Checks UI : https://localhost:5000/health-ui

- **Auth.API** : https://localhost:5001
  - Documentation Swagger : https://localhost:5001/swagger

- **Order.API** : https://localhost:5002
  - Documentation Swagger : https://localhost:5002/swagger

- **Catalog.API** : https://localhost:5003
  - Documentation Swagger : https://localhost:5003/swagger

## Avantages de cette Approche

✅ **Stabilité** : Chaque service s'exécute dans son propre terminal externe
✅ **Isolation** : Les services ne s'interrompent pas mutuellement
✅ **Monitoring** : Vue claire de chaque service individuellement
✅ **Debugging** : Logs visibles en temps réel pour chaque service
✅ **Flexibilité** : Possibilité de redémarrer un service spécifique