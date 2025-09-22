# PLAN DE RECONSTRUCTION RESTAURANT.API
# =====================================
# NiesPro ERP - Phase de Reconstruction Méthodique
# 90+ erreurs architecturales à résoudre

## 🎯 OBJECTIF
Reconstruction complète du service Restaurant.API pour atteindre 100% de fonctionnalités opérationnelles dans l'ERP NiesPro.

## 📊 DIAGNOSTIC ACTUEL
- **Erreurs identifiées**: 90+ erreurs de compilation
- **Types d'erreurs**:
  - Événements de domaine manquants (15+)
  - Objets de valeur incomplets (PreparationTime, WorkSchedule)
  - Définitions d'énumérations incomplètes
  - Conflits de dépendances (MediatR 12.2.0 vs 12.4.1)
  - Références manquantes dans la couche Application

## 🏗️ ARCHITECTURE CIBLE
```
Restaurant.API/
├── Restaurant.Domain/          # Logique métier pure
│   ├── Entities/              # Entités principales
│   │   ├── Restaurant.cs      ✅ Existe
│   │   ├── Table.cs           ✅ Existe  
│   │   ├── Order.cs           🔄 À réviser
│   │   ├── MenuItem.cs        ✅ Existe
│   │   ├── Staff.cs           ✅ Existe
│   │   └── Reservation.cs     ✅ Existe
│   ├── ValueObjects/          # Objets de valeur
│   │   ├── PreparationTime.cs ❌ Manquant
│   │   ├── WorkSchedule.cs    ❌ Manquant
│   │   ├── TableCapacity.cs   ❌ Manquant
│   │   └── MenuPrice.cs       ❌ Manquant
│   ├── Events/                # Événements de domaine
│   │   ├── TableReserved.cs   ❌ Manquant
│   │   ├── OrderPlaced.cs     ❌ Manquant
│   │   ├── MenuItemCreated.cs ❌ Manquant
│   │   └── StaffShiftStarted.cs ❌ Manquant
│   └── Enums/                 # Énumérations
│       ├── TableStatus.cs     🔄 Incomplet
│       ├── OrderStatus.cs     🔄 Incomplet
│       └── StaffRole.cs       🔄 Incomplet
├── Restaurant.Application/     # Logique applicative
│   ├── Commands/              # Commandes CQRS
│   ├── Queries/               # Requêtes CQRS
│   ├── Handlers/              # Gestionnaires MediatR
│   └── DTOs/                  # Objets de transfert
├── Restaurant.Infrastructure/  # Accès aux données
│   ├── Data/                  # EF Core DbContext
│   ├── Repositories/          # Implémentations Repository
│   └── Services/              # Services externes
└── Restaurant.API/            # Point d'entrée REST
    ├── Controllers/           # API Controllers
    ├── Program.cs             🔄 Configuration
    └── appsettings.json       ✅ Configuré
```

## 📋 PLAN D'EXÉCUTION (2-3 JOURS)

### JOUR 1: FONDATIONS DOMAIN
**Matin (4h)**
1. **Correction des Énumérations** (1h)
   - Compléter TableStatus, OrderStatus, StaffRole
   - Ajouter toutes les valeurs métier requises
   
2. **Création des Value Objects** (2h)
   - Implémenter PreparationTime avec validation
   - Créer WorkSchedule avec horaires/jours
   - Développer TableCapacity avec contraintes
   - Construire MenuPrice avec devise

3. **Révision des Entités** (1h)
   - Corriger les conflits Entity.CreatedAt
   - Valider les relations entre entités
   - Appliquer les conventions Enhanced Entity

**Après-midi (4h)**
4. **Événements de Domaine** (3h)
   - TableReserved, TableFreed
   - OrderPlaced, OrderCompleted, OrderCancelled
   - MenuItemCreated, MenuItemUpdated
   - StaffShiftStarted, StaffShiftEnded
   - ReservationConfirmed, ReservationCancelled

5. **Tests Unitaires Domain** (1h)
   - Tests des Value Objects
   - Tests des règles métier

### JOUR 2: APPLICATION & INFRASTRUCTURE
**Matin (4h)**
1. **Configuration MediatR** (1h)
   - Mise à jour vers 12.4.1
   - Configuration des handlers
   - Pipeline de validation

2. **Commandes CQRS** (2h)
   - CreateTableCommand
   - ReserveTableCommand
   - PlaceOrderCommand
   - CreateMenuItemCommand
   - ScheduleStaffCommand

3. **Requêtes CQRS** (1h)
   - GetTablesQuery
   - GetAvailableTablesQuery
   - GetOrdersByTableQuery
   - GetMenuQuery

**Après-midi (4h)**
4. **Handlers & DTOs** (2h)
   - Implémentation des handlers
   - Mapping entités ↔ DTOs
   - Validation métier

5. **Infrastructure EF Core** (2h)
   - Configuration DbContext
   - Migrations de base de données
   - Repositories concrets

### JOUR 3: API & INTÉGRATION
**Matin (4h)**
1. **Controllers REST** (2h)
   - TablesController
   - OrdersController  
   - MenuController
   - ReservationsController
   - StaffController

2. **Configuration API** (1h)
   - Program.cs complet
   - Health checks
   - Swagger documentation

3. **Tests d'Intégration** (1h)
   - Tests des endpoints
   - Validation des responses

**Après-midi (4h)**
4. **Déploiement & Validation** (2h)
   - Configuration Docker
   - Tests de déploiement
   - Validation avec autres services

5. **Documentation & Finalisation** (2h)
   - Documentation API
   - Tests de performance
   - Validation finale du système

## 🔧 COMMANDES DE DÉVELOPPEMENT

### Phase 1: Diagnostic Détaillé
```bash
cd src/Services/Restaurant/Restaurant.API
dotnet build --verbosity detailed > build-errors.log 2>&1
```

### Phase 2: Correction Progressive
```bash
# Corriger une catégorie d'erreurs à la fois
dotnet build | grep "CS[0-9]" | sort | uniq -c
```

### Phase 3: Validation Finale
```bash
dotnet test
dotnet run --launch-profile Development
curl http://localhost:7001/health
```

## 📊 MÉTRIQUES DE SUCCÈS
- **0 erreurs de compilation** ✅
- **Tous les tests unitaires passent** ✅
- **Health endpoint répond** ✅
- **Documentation Swagger générée** ✅
- **Intégration avec les autres services** ✅

## 🎯 RÉSULTAT ATTENDU
Au terme de cette reconstruction:
- **Restaurant.API**: 100% opérationnel ✅
- **NiesPro ERP**: 100% de fonctionnalités critiques ✅
- **Architecture**: Clean, maintenable, extensible ✅
- **Performance**: Optimisée pour production ✅

## 📞 SUPPORT TECHNIQUE
- **Estimation**: 2-3 jours de développement intensif
- **Complexité**: Élevée (90+ erreurs architecturales)
- **Risques**: Faibles (architecture bien définie)
- **ROI**: Très élevé (passage de 83% à 100% opérationnel)

---
**Note**: Cette reconstruction représente la phase finale pour atteindre un ERP NiesPro 100% fonctionnel avec architecture microservices complète.