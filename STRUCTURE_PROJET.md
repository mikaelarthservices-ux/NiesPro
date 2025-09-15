# Structure du projet NiesPro ERP

## 📁 Arborescence complète

```
NiesPro/
├── .github/                           # GitHub Actions workflows
│   ├── workflows/
│   │   ├── ci-cd.yml                 # Pipeline CI/CD
│   │   ├── security-scan.yml         # Scan sécurité
│   │   └── performance-tests.yml     # Tests performance
│   └── PULL_REQUEST_TEMPLATE.md      # Template PR
│
├── docs/                             # Documentation projet
│   ├── ARCHITECTURE.md               # Architecture détaillée
│   ├── API.md                       # Documentation API
│   ├── DEPLOYMENT.md                # Guide déploiement
│   ├── DEVELOPMENT.md               # Guide développement
│   ├── SECURITY.md                  # Guide sécurité
│   └── USER_GUIDE.md               # Guide utilisateur
│
├── scripts/                         # Scripts utilitaires
│   ├── database/
│   │   ├── migrations/              # Scripts migration DB
│   │   ├── seeds/                   # Données de test
│   │   └── init.sql                # Initialisation DB
│   ├── deployment/
│   │   ├── docker-compose.yml       # Développement local
│   │   ├── docker-compose.prod.yml  # Production
│   │   └── kubernetes/              # Manifests K8s
│   └── tools/
│       ├── generate-certs.sh        # Génération certificats
│       └── backup-restore.sh        # Sauvegarde/restauration
│
├── src/                            # Code source principal
│   ├── BuildingBlocks/             # Composants partagés
│   │   ├── Common/
│   │   │   ├── NiesPro.Common.csproj
│   │   │   ├── Models/              # DTOs et modèles partagés
│   │   │   ├── Extensions/          # Extensions utilitaires
│   │   │   ├── Helpers/            # Classes d'aide
│   │   │   └── Constants/          # Constantes globales
│   │   ├── EventBus/
│   │   │   ├── NiesPro.EventBus.csproj
│   │   │   ├── Abstractions/       # Interfaces event bus
│   │   │   ├── RabbitMQ/           # Implémentation RabbitMQ
│   │   │   └── Events/             # Événements partagés
│   │   ├── Infrastructure/
│   │   │   ├── NiesPro.Infrastructure.csproj
│   │   │   ├── Authentication/     # Middleware auth
│   │   │   ├── Authorization/      # Policies et handlers
│   │   │   ├── Caching/           # Abstraction cache Redis
│   │   │   ├── Database/          # Context et patterns
│   │   │   ├── Files/             # Gestion fichiers
│   │   │   ├── Logging/           # Configuration Serilog
│   │   │   ├── Messaging/         # Notification services
│   │   │   └── Security/          # Chiffrement et sécurité
│   │   └── WebApi/
│   │       ├── NiesPro.WebApi.csproj
│   │       ├── Middleware/         # Middlewares communs
│   │       ├── Filters/           # Action filters
│   │       ├── Extensions/        # Extensions ASP.NET
│   │       └── Swagger/           # Configuration Swagger
│   │
│   ├── Services/                   # Microservices
│   │   ├── Auth/
│   │   │   ├── Auth.API/
│   │   │   │   ├── Auth.API.csproj
│   │   │   │   ├── Controllers/
│   │   │   │   │   ├── AuthController.cs
│   │   │   │   │   ├── DeviceController.cs
│   │   │   │   │   └── UserController.cs
│   │   │   │   ├── Program.cs
│   │   │   │   ├── Startup.cs
│   │   │   │   ├── appsettings.json
│   │   │   │   └── Dockerfile
│   │   │   ├── Auth.Domain/
│   │   │   │   ├── Auth.Domain.csproj
│   │   │   │   ├── Entities/       # Entités métier
│   │   │   │   ├── ValueObjects/   # Value objects
│   │   │   │   ├── Interfaces/     # Interfaces domaine
│   │   │   │   └── Services/       # Services domaine
│   │   │   ├── Auth.Application/
│   │   │   │   ├── Auth.Application.csproj
│   │   │   │   ├── Commands/       # CQRS Commands
│   │   │   │   ├── Queries/        # CQRS Queries
│   │   │   │   ├── Handlers/       # Command/Query handlers
│   │   │   │   ├── DTOs/          # Data Transfer Objects
│   │   │   │   ├── Validators/     # FluentValidation
│   │   │   │   └── Services/       # Services applicatifs
│   │   │   └── Auth.Infrastructure/
│   │   │       ├── Auth.Infrastructure.csproj
│   │   │       ├── Data/
│   │   │       │   ├── AuthDbContext.cs
│   │   │       │   ├── Configurations/  # EF configurations
│   │   │       │   └── Migrations/      # EF migrations
│   │   │       ├── Repositories/        # Implémentation repos
│   │   │       └── Services/           # Services infrastructure
│   │   │
│   │   ├── Product/
│   │   │   ├── Product.API/
│   │   │   ├── Product.Domain/
│   │   │   ├── Product.Application/
│   │   │   └── Product.Infrastructure/
│   │   │
│   │   ├── Stock/
│   │   │   ├── Stock.API/
│   │   │   ├── Stock.Domain/
│   │   │   ├── Stock.Application/
│   │   │   └── Stock.Infrastructure/
│   │   │
│   │   ├── Order/
│   │   │   ├── Order.API/
│   │   │   ├── Order.Domain/
│   │   │   ├── Order.Application/
│   │   │   └── Order.Infrastructure/
│   │   │
│   │   ├── Payment/
│   │   │   ├── Payment.API/
│   │   │   ├── Payment.Domain/
│   │   │   ├── Payment.Application/
│   │   │   └── Payment.Infrastructure/
│   │   │
│   │   ├── Customer/
│   │   │   ├── Customer.API/
│   │   │   ├── Customer.Domain/
│   │   │   ├── Customer.Application/
│   │   │   └── Customer.Infrastructure/
│   │   │
│   │   ├── Restaurant/
│   │   │   ├── Restaurant.API/
│   │   │   ├── Restaurant.Domain/
│   │   │   ├── Restaurant.Application/
│   │   │   └── Restaurant.Infrastructure/
│   │   │
│   │   ├── Notification/
│   │   │   ├── Notification.API/
│   │   │   ├── Notification.Domain/
│   │   │   ├── Notification.Application/
│   │   │   └── Notification.Infrastructure/
│   │   │
│   │   ├── File/
│   │   │   ├── File.API/
│   │   │   ├── File.Domain/
│   │   │   ├── File.Application/
│   │   │   └── File.Infrastructure/
│   │   │
│   │   ├── Report/
│   │   │   ├── Report.API/
│   │   │   ├── Report.Domain/
│   │   │   ├── Report.Application/
│   │   │   └── Report.Infrastructure/
│   │   │
│   │   └── Log/
│   │       ├── Log.API/
│   │       ├── Log.Domain/
│   │       ├── Log.Application/
│   │       └── Log.Infrastructure/
│   │
│   ├── Gateways/                   # API Gateways
│   │   ├── Web.Gateway/
│   │   │   ├── Web.Gateway.csproj
│   │   │   ├── Program.cs
│   │   │   ├── ocelot.json         # Configuration Ocelot
│   │   │   └── Dockerfile
│   │   └── Mobile.Gateway/
│   │       ├── Mobile.Gateway.csproj
│   │       ├── Program.cs
│   │       ├── ocelot.json
│   │       └── Dockerfile
│   │
│   └── Clients/                    # Applications clientes
│       ├── Desktop/
│       │   ├── NiesPro.Desktop.csproj
│       │   ├── MainWindow.xaml
│       │   ├── App.xaml
│       │   ├── Views/
│       │   │   ├── Auth/           # Écrans authentification
│       │   │   ├── POS/            # Interface caisse
│       │   │   ├── Stock/          # Gestion stock
│       │   │   ├── Admin/          # Administration
│       │   │   └── Shared/         # Composants partagés
│       │   ├── ViewModels/         # MVVM ViewModels
│       │   ├── Services/           # Services clients
│       │   ├── Models/            # Modèles client
│       │   └── Resources/         # Ressources (images, styles)
│       │
│       ├── Mobile/
│       │   ├── NiesPro.Mobile.csproj
│       │   ├── Platforms/
│       │   │   ├── Android/
│       │   │   └── iOS/
│       │   ├── Views/
│       │   │   ├── AuthPage.xaml
│       │   │   ├── OrderPage.xaml
│       │   │   ├── MenuPage.xaml
│       │   │   └── KitchenPage.xaml
│       │   ├── ViewModels/
│       │   ├── Services/
│       │   └── Resources/
│       │
│       └── Web/
│           ├── NiesPro.Web.csproj
│           ├── Program.cs
│           ├── Pages/
│           │   ├── Index.razor
│           │   ├── Auth/
│           │   ├── Dashboard/
│           │   ├── Reports/
│           │   └── Admin/
│           ├── Components/
│           │   ├── Charts/
│           │   ├── Tables/
│           │   └── Forms/
│           ├── Services/
│           └── wwwroot/
│               ├── css/
│               ├── js/
│               └── images/
│
├── tests/                          # Tests du projet
│   ├── Unit/
│   │   ├── Auth.Tests/
│   │   │   ├── Auth.Tests.csproj
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   └── API/
│   │   ├── Product.Tests/
│   │   ├── Stock.Tests/
│   │   └── ...
│   │
│   ├── Integration/
│   │   ├── Auth.Integration.Tests/
│   │   │   ├── Auth.Integration.Tests.csproj
│   │   │   ├── API/
│   │   │   └── Database/
│   │   ├── Product.Integration.Tests/
│   │   └── ...
│   │
│   ├── E2E/
│   │   ├── NiesPro.E2E.Tests.csproj
│   │   ├── Desktop/
│   │   │   ├── POS/
│   │   │   └── Admin/
│   │   ├── Mobile/
│   │   │   └── OrderFlow/
│   │   └── Web/
│   │       ├── Dashboard/
│   │       └── Reports/
│   │
│   └── Performance/
│       ├── NiesPro.Performance.Tests.csproj
│       ├── LoadTests/
│       │   ├── AuthLoadTest.cs
│       │   ├── OrderLoadTest.cs
│       │   └── StockLoadTest.cs
│       └── Benchmarks/
│           ├── ProductBenchmarks.cs
│           └── DatabaseBenchmarks.cs
│
├── tools/                          # Outils de développement
│   ├── CodeGeneration/
│   │   ├── ServiceGenerator/       # Générateur microservices
│   │   └── ApiGenerator/          # Générateur APIs
│   ├── Migration/
│   │   ├── DataMigration/         # Migration données
│   │   └── SchemaUpdater/         # Mise à jour schémas
│   └── Monitoring/
│       ├── HealthCheckDashboard/  # Dashboard health checks
│       └── MetricsCollector/      # Collecteur métriques
│
├── .editorconfig                   # Configuration éditeur
├── .gitignore                     # Git ignore rules
├── .gitattributes                 # Git attributes
├── Directory.Build.props          # Propriétés MSBuild globales
├── Directory.Build.targets        # Targets MSBuild globales
├── global.json                    # Configuration .NET SDK
├── NiesPro.sln                    # Solution principale
├── nuget.config                   # Configuration NuGet
├── README.md                      # Documentation principale
├── CHANGELOG.md                   # Historique des versions
├── CONTRIBUTING.md                # Guide contribution
├── LICENSE                        # Licence du projet
├── SECURITY.md                    # Politique sécurité
└── docker-compose.yml             # Docker Compose développement
```

## 📋 Fichiers de configuration principaux

### Directory.Build.props
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <Company>NiesPro</Company>
    <Product>NiesPro ERP</Product>
    <Copyright>Copyright © NiesPro 2025</Copyright>
    <Version>1.0.0</Version>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" Version="6.0.0" />
    <PackageReference Include="Serilog" Version="2.12.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
  </ItemGroup>
</Project>
```

### global.json
```json
{
  "sdk": {
    "version": "6.0.400",
    "rollForward": "latestMinor"
  },
  "msbuild-sdks": {
    "Microsoft.Build.Traversal": "3.1.6"
  }
}
```

### .editorconfig
```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

[*.{yml,yaml}]
indent_size = 2

[*.json]
indent_size = 2

[*.cs]
# Règles de style C#
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
```

## 🏗️ Conventions de structure

### Microservice standard
Chaque microservice suit la structure Clean Architecture :

```
Service.Name/
├── Service.Name.API/          # Couche présentation (Controllers, Program.cs)
├── Service.Name.Domain/       # Couche domaine (Entities, Value Objects, Interfaces)
├── Service.Name.Application/  # Couche application (Use Cases, DTOs, Validators)
└── Service.Name.Infrastructure/ # Couche infrastructure (Data, External Services)
```

### Naming conventions
- **Namespaces** : `NiesPro.Services.Auth.Domain`
- **Classes** : PascalCase (`ProductService`, `OrderEntity`)
- **Méthodes** : PascalCase (`GetProductByIdAsync`)
- **Variables** : camelCase (`productId`, `orderItems`)
- **Constantes** : UPPER_CASE (`MAX_RETRY_COUNT`)
- **Interfaces** : Préfixe I (`IProductRepository`)

### Organisation des dossiers
- **Pluriel** pour les dossiers de collections (`Controllers`, `Services`, `Models`)
- **Singulier** pour les dossiers conceptuels (`Domain`, `Infrastructure`)
- **Groupement logique** par fonctionnalité plutôt que par type technique

## 🔧 Configuration des projets

### Exemple de .csproj pour API
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AspNetCore.HealthChecks.MySql" Version="6.0.2" />
    <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="6.0.4" />
    <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.10" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Auth.Application\Auth.Application.csproj" />
    <ProjectReference Include="..\Auth.Infrastructure\Auth.Infrastructure.csproj" />
    <ProjectReference Include="..\..\BuildingBlocks\WebApi\NiesPro.WebApi.csproj" />
  </ItemGroup>

</Project>
```

Cette structure de projet fournit une organisation claire, maintenable et évolutive pour le développement professionnel du système NiesPro ERP, respectant les meilleures pratiques de l'industrie.

---

**Structure validée le :** [Date]  
**Architecte :** [Nom]
