# Simple Service Manager - NiesPro

## Description
Interface graphique simplifiée pour gérer tous les microservices de NiesPro. Résout les problèmes de "running puis stopped" et d'affichage des logs.

## Fonctionnalités
- ✅ Démarrage/Arrêt individuel ou groupé des services
- ✅ Affichage des logs en temps réel avec synchronisation
- ✅ Monitoring du statut des services (Running/Stopped/Starting/Error)
- ✅ Accès rapide aux endpoints Health et Swagger
- ✅ Interface utilisateur simple et intuitive
- ✅ Gestion robuste des processus

## Services Gérés
- **Gateway.API** (Port 5000) - API Gateway principal
- **Auth.API** (Port 5001) - Service d'authentification  
- **Order.API** (Port 5002) - Service de commandes
- **Catalog.API** (Port 5003) - Service de catalogue

## Comment utiliser

### Option 1: Script automatique
Double-cliquez sur `build-and-start-simple.bat` à la racine du projet

### Option 2: Manuel
```bash
cd tools/SimpleServiceManager
dotnet build
dotnet run
```

## Interface

### Panneau Services (Gauche)
- Statut coloré de chaque service (Vert=Running, Rouge=Stopped, Orange=Starting)
- Boutons d'action pour chaque service:
  - **Start**: Démarre le service
  - **Stop**: Arrête le service  
  - **Health**: Ouvre l'endpoint de santé
  - **Swagger**: Ouvre la documentation API

### Panneau Logs (Droite)
- Sélection du service via dropdown
- Logs en temps réel avec timestamps
- Bouton Clear pour nettoyer les logs
- Auto-scroll optionnel

### Barre d'actions (Haut)
- **Start All**: Démarre tous les services
- **Stop All**: Arrête tous les services

## Avantages vs Version Complexe
- ✅ **Simplicité**: Interface épurée sans Material Design
- ✅ **Stabilité**: Gestion directe des processus sans couches d'abstraction
- ✅ **Logs fonctionnels**: Capture réelle de stdout/stderr
- ✅ **Rapidité**: Compilation et lancement plus rapides
- ✅ **Débogage facile**: Code simple et lisible

## Architecture Technique
- **WPF .NET 8**: Interface graphique native Windows
- **Process Management**: Gestion directe via System.Diagnostics.Process
- **Real-time Logging**: Capture des streams avec StringBuilder thread-safe
- **HTTP Health Checks**: Monitoring via HttpClient
- **Observer Pattern**: INotifyPropertyChanged pour les mises à jour UI

## Dépendances
- .NET 8 Windows Desktop
- Microsoft.Extensions.Http
- Newtonsoft.Json

## Résolution des Problèmes

### "Services show Running then Stopped"
✅ **Résolu**: Le Simple Service Manager utilise une gestion robuste des processus avec monitoring continu

### "Logs n'affichent pas"  
✅ **Résolu**: Capture directe des streams stdout/stderr avec synchronisation thread-safe

### PowerShell Execution Policy
✅ **Contourné**: Plus besoin de scripts PowerShell, tout est géré en C#