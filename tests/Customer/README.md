# Customer Service Tests

## Structure des Tests

Ce dossier contient tous les tests pour le service Customer, organisés selon les standards NiesPro Enterprise.

### Organisation

```
tests/Customer/
├── Unit/                           # Tests unitaires
│   ├── Application/               # Tests des handlers (Commands/Queries)
│   │   ├── CreateCustomerCommandHandlerTests.cs
│   │   └── GetCustomerByIdQueryHandlerTests.cs
│   ├── Domain/                    # Tests des entités de domaine
│   │   ├── CustomerTests.cs
│   │   └── CustomerAddressTests.cs
│   └── Customer.Tests.Unit.csproj # Projet de tests unitaires
├── Integration/                   # Tests d'intégration (à implémenter)
└── README.md                      # Ce fichier
```

## Standards de Test

### Patterns Utilisés

1. **NUnit** comme framework de test
2. **FluentAssertions** pour les assertions expressives
3. **Moq** pour les mocks
4. **AutoFixture** pour la génération de données de test
5. **AAA Pattern** : Arrange, Act, Assert

### Conventions

1. **Nommage des tests** : `Method_Scenario_ExpectedResult`
2. **Setup standard** avec mocks de toutes les dépendances
3. **Tests de logging** pour vérifier l'intégration NiesPro.Logging.Client
4. **Tests d'audit** pour vérifier l'intégration IAuditServiceClient
5. **Tests d'exception** pour la gestion d'erreur

### Coverage Requirements

- **Application Layer** : 100% des handlers testés
- **Domain Layer** : 100% des méthodes métier testées
- **Edge Cases** : Tests des cas d'erreur et exceptions
- **Integration** : Tests de logging et audit

## Exécution des Tests

### Commande de base
```bash
dotnet test tests/Customer/Unit/Customer.Tests.Unit.csproj
```

### Avec coverage
```bash
dotnet test tests/Customer/Unit/Customer.Tests.Unit.csproj --collect:"XPlat Code Coverage"
```

### Tous les tests Customer
```bash
dotnet test tests/Customer/ --configuration Release
```

## Tests Implémentés

### ✅ Application Layer

- **CreateCustomerCommandHandler**
  - ✅ Création réussie avec données valides
  - ✅ Échec si email existe déjà  
  - ✅ Gestion des adresses multiples
  - ✅ Gestion des exceptions repository
  - ✅ Vérification du logging et audit

- **GetCustomerByIdQueryHandler**
  - ✅ Récupération réussie par ID existant
  - ✅ Échec pour ID non existant
  - ✅ Gestion des exceptions repository
  - ✅ Vérification du logging
  - ✅ Validation des ID invalides

### ✅ Domain Layer

- **Customer Entity**
  - ✅ Valeurs par défaut à la création
  - ✅ Propriété calculée FullName
  - ✅ Gestion des points de fidélité (ajout/utilisation)
  - ✅ Enregistrement des visites et commandes
  - ✅ Promotion VIP et gestion du statut actif

- **CustomerAddress Entity**
  - ✅ Valeurs par défaut à la création
  - ✅ Configuration complète des propriétés
  - ✅ Propriétés minimales requises
  - ✅ Différents types d'adresse

## Tests à Implémenter

### 🔄 À Venir

- **Integration Tests** : Tests bout-en-bout avec base de données
- **Performance Tests** : Tests de charge sur les handlers
- **API Tests** : Tests des controllers (si applicable)

## Qualité et Standards

Ce projet de test suit exactement les mêmes patterns que :
- ✅ **Auth.Tests.Unit** : Même structure, mêmes conventions
- ✅ **Catalog.Tests.Unit** : Même organisation, même approche
- ✅ **NiesPro Enterprise Standards** : Logging, audit, error handling

Les tests garantissent la conformité du service Customer avec l'architecture NiesPro Enterprise.