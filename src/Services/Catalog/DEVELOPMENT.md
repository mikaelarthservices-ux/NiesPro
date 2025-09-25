# Guide de Développement - Catalog.API

## 🛠️ Environnement de développement

### Prérequis
- **Visual Studio 2022** ou **VS Code** avec extensions C#
- **.NET 8 SDK** (version 8.0.100 minimum)
- **MySQL 9.1.0** (WAMP64 ou installation native)
- **Git** pour versioning
- **PowerShell 7+** pour scripts de maintenance

### Extensions VS Code recommandées
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscodeintellicode-csharp", 
    "formulahendry.dotnet-test-explorer",
    "humao.rest-client",
    "ms-vscode.powershell"
  ]
}
```

### Configuration launch.json
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Catalog.API",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Services/Catalog/Catalog.API/bin/Debug/net8.0/Catalog.API.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/Services/Catalog/Catalog.API",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
        "uriFormat": "%s/swagger"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

## 🚀 Scripts de développement

### 1. Script de démarrage rapide
**Fichier** : `dev-start.ps1`
```powershell
# Démarrage complet de l'environnement Catalog
Write-Host "🚀 Démarrage environnement Catalog.API" -ForegroundColor Green

# Vérification prérequis
Write-Host "📋 Vérification des prérequis..."
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK non trouvé!"
    exit 1
}

# Test MySQL
$mysqlPath = "C:\wamp64\bin\mysql\mysql9.1.0\bin\mysql.exe"
if (-not (Test-Path $mysqlPath)) {
    Write-Warning "MySQL non trouvé à $mysqlPath"
}

# Restauration packages
Write-Host "📦 Restauration des packages..."
dotnet restore src/Services/Catalog/Catalog.API/

# Compilation
Write-Host "🔨 Compilation..."
dotnet build src/Services/Catalog/Catalog.API/ --no-restore

# Application migrations
Write-Host "🗄️ Application des migrations..."
dotnet ef database update --project src/Services/Catalog/Catalog.Infrastructure/ --startup-project src/Services/Catalog/Catalog.API/

# Test rapide
Write-Host "🧪 Test de connectivité..."
& "tools\catalog-db-inspector.ps1" -Action "test"

# Démarrage service
Write-Host "🎯 Démarrage du service sur http://localhost:5003"
Write-Host "📚 Swagger disponible sur http://localhost:5003/swagger"
dotnet run --project src/Services/Catalog/Catalog.API/
```

### 2. Script de tests automatisés
**Fichier** : `dev-test.ps1`
```powershell
# Suite complète de tests Catalog.API
param(
    [string]$TestType = "all",
    [switch]$Coverage
)

Write-Host "🧪 Exécution des tests Catalog.API" -ForegroundColor Cyan

switch ($TestType) {
    "unit" {
        Write-Host "🔬 Tests unitaires..."
        dotnet test tests/Catalog.Tests.Unit/ --logger "console;verbosity=detailed"
    }
    "integration" {
        Write-Host "🔗 Tests d'intégration..."
        & "tools\catalog-service-tester.ps1"
    }
    "db" {
        Write-Host "🗄️ Tests base de données..."
        & "tools\catalog-db-inspector.ps1" -Action "all"
    }
    "all" {
        Write-Host "🎯 Suite complète de tests..."
        # Tests unitaires
        if (Test-Path "tests/Catalog.Tests.Unit/") {
            dotnet test tests/Catalog.Tests.Unit/
        }
        
        # Tests DB
        & "tools\catalog-db-inspector.ps1" -Action "test"
        
        # Tests API
        & "tools\catalog-service-tester.ps1"
        
        Write-Host "✅ Tests terminés!" -ForegroundColor Green
    }
}

if ($Coverage) {
    Write-Host "📊 Génération rapport de couverture..."
    dotnet test --collect:"XPlat Code Coverage" --results-directory:"./coverage"
}
```

### 3. Script de nettoyage
**Fichier** : `dev-clean.ps1`
```powershell
# Nettoyage environnement développement
Write-Host "🧹 Nettoyage environnement Catalog.API" -ForegroundColor Yellow

# Arrêt des processus
Write-Host "⏹️ Arrêt des processus dotnet..."
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*Catalog*" } | Stop-Process -Force

# Nettoyage binaires
Write-Host "🗑️ Suppression des fichiers temporaires..."
Remove-Item -Path "src/Services/Catalog/*/bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Services/Catalog/*/obj" -Recurse -Force -ErrorAction SilentlyContinue

# Nettoyage base de test
Write-Host "🗄️ Nettoyage base de données de test..."
# Script SQL de nettoyage si nécessaire

# Nettoyage packages
Write-Host "📦 Nettoyage cache NuGet..."
dotnet nuget locals all --clear

Write-Host "✅ Nettoyage terminé!" -ForegroundColor Green
```

## 🔧 Configuration IDE

### Paramètres EditorConfig
**Fichier** : `.editorconfig`
```ini
[*.{cs,csx}]
# Indentation
indent_style = space
indent_size = 4

# Nouvelle ligne
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true

# Conventions de nommage C#
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.severity = warning
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.symbols = interface
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.style = prefix_interface_with_i

# Règles de style
dotnet_style_qualification_for_field = false:warning
dotnet_style_qualification_for_property = false:warning
csharp_prefer_braces = true:warning
csharp_style_var_for_built_in_types = false:suggestion
```

### Configuration Omnisharp (VS Code)
**Fichier** : `omnisharp.json`
```json
{
  "FormattingOptions": {
    "NewLinesForBracesInMethods": false,
    "NewLinesForBracesInProperties": false,
    "NewLinesForBracesInAccessors": false,
    "NewLinesForBracesInAnonymousMethods": false,
    "NewLinesForBracesInControlBlocks": false,
    "NewLinesForBracesInAnonymousTypes": false,
    "NewLinesForBracesInObjectCollectionArrayInitializers": false,
    "NewLinesForBracesInLambdaExpressionBody": false,
    "NewLineForElse": false,
    "NewLineForCatch": false,
    "NewLineForFinally": false
  },
  "RoslynExtensionsOptions": {
    "EnableAnalyzersSupport": true,
    "LocationPaths": ["./analyzers"]
  }
}
```

## 📚 Ressources et documentation

### Liens utiles
- **Clean Architecture** : [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- **CQRS Pattern** : [Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- **Entity Framework** : [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- **FluentValidation** : [Documentation officielle](https://docs.fluentvalidation.net/)
- **MediatR** : [GitHub Repository](https://github.com/jbogard/MediatR)

### Commandes utiles

#### EF Core Migrations
```bash
# Créer une nouvelle migration
dotnet ef migrations add <MigrationName> --project Catalog.Infrastructure --startup-project Catalog.API

# Appliquer les migrations
dotnet ef database update --project Catalog.Infrastructure --startup-project Catalog.API

# Générer script SQL
dotnet ef migrations script --project Catalog.Infrastructure --startup-project Catalog.API

# Annuler la dernière migration
dotnet ef migrations remove --project Catalog.Infrastructure --startup-project Catalog.API
```

#### Tests et diagnostics
```bash
# Exécuter tests avec couverture
dotnet test --collect:"XPlat Code Coverage"

# Analyser performance
dotnet run --project Catalog.API --configuration Release

# Profiling mémoire
dotnet-counters monitor --process-id <PID> System.Runtime

# Audit sécurité packages
dotnet list package --vulnerable --include-transitive
```

## 🐛 Debugging et troubleshooting

### Problèmes courants

#### 1. Erreur de connexion MySQL
**Symptômes** : `MySql.Data.MySqlClient.MySqlException: Unable to connect`
**Solutions** :
```powershell
# Vérifier MySQL
& "tools\catalog-db-inspector.ps1" -Action "test"

# Vérifier configuration
Get-Content "src/Services/Catalog/Catalog.API/appsettings.json" | Select-String "ConnectionStrings"

# Redémarrer MySQL (WAMP)
Restart-Service -Name "wampmysqld64"
```

#### 2. Erreur de migration EF
**Symptômes** : `The database does not exist`
**Solutions** :
```bash
# Recréer la base
dotnet ef database drop --project Catalog.Infrastructure --startup-project Catalog.API
dotnet ef database update --project Catalog.Infrastructure --startup-project Catalog.API

# Ou utiliser le script setup
& "tools\catalog-db-setup.ps1"
```

#### 3. Port déjà utilisé
**Symptômes** : `Address already in use`
**Solutions** :
```powershell
# Trouver le processus
netstat -ano | findstr :5003

# Tuer le processus
Stop-Process -Id <PID> -Force
```

### Logging et monitoring

#### Configuration Serilog
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/catalog-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

#### Points de monitoring
- **Performance** : Temps de réponse > 500ms
- **Erreurs** : Exceptions non gérées
- **Base de données** : Timeouts, deadlocks
- **Mémoire** : Consommation > 500MB

## 🔄 Workflow de développement

### 1. Nouvelle fonctionnalité
```bash
# 1. Créer branche feature
git checkout -b feature/catalog-search-optimization

# 2. Développement avec tests
# - Écrire tests d'abord (TDD)
# - Implémenter fonctionnalité
# - Valider avec scripts

# 3. Tests complets
& "dev-test.ps1" -TestType "all"

# 4. Commit et push
git add .
git commit -m "feat: optimize product search with indexes"
git push origin feature/catalog-search-optimization
```

### 2. Correction de bug
```bash
# 1. Reproduire le bug avec test
# 2. Créer test qui échoue
# 3. Corriger le code
# 4. Valider que le test passe
# 5. Exécuter suite complète
```

### 3. Refactoring
```bash
# 1. S'assurer que tous les tests passent
& "dev-test.ps1"

# 2. Refactorer avec confiance
# 3. Re-exécuter tests après chaque étape
# 4. Validation performance si nécessaire
```

---

**Guide de Développement Catalog.API** - Productivité maximale 🛠️⚡