# =======================================================================
# CATALOG DATABASE SETUP - NiesPro
# Script pour créer et initialiser la base de données Catalog
# =======================================================================

param(
    [switch]$Force,
    [switch]$Seed,
    [switch]$Reset,
    [string]$Environment = "Development"
)

Write-Host "🚀 CATALOG DATABASE SETUP" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

$catalogPath = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Catalog\Catalog.API"

# Vérifier que le projet existe
if (-not (Test-Path $catalogPath)) {
    Write-Host "❌ Projet Catalog.API non trouvé: $catalogPath" -ForegroundColor Red
    exit 1
}

# Changer vers le répertoire du projet
Set-Location $catalogPath
Write-Host "📁 Répertoire de travail: $catalogPath" -ForegroundColor Cyan

# Fonction pour exécuter une commande dotnet
function Invoke-DotnetCommand {
    param([string]$Command, [string]$Description)
    
    Write-Host "🔧 $Description..." -ForegroundColor Yellow
    Write-Host "   Commande: dotnet $Command" -ForegroundColor Gray
    
    try {
        $commandArgs = $Command.Split(' ')
        $result = & dotnet $commandArgs 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $Description réussi" -ForegroundColor Green
            if ($result) {
                Write-Host $result -ForegroundColor White
            }
            return $true
        } else {
            Write-Host "❌ $Description échoué" -ForegroundColor Red
            Write-Host $result -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Erreur lors de $Description : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Étape 1: Compilation du projet
Write-Host "`n📦 ÉTAPE 1: Compilation du projet" -ForegroundColor Magenta
if (-not (Invoke-DotnetCommand "build --no-restore" "Compilation")) {
    Write-Host "❌ Impossible de continuer sans compilation réussie" -ForegroundColor Red
    exit 1
}

# Étape 2: Vérification des migrations
Write-Host "`n🔍 ÉTAPE 2: Vérification des migrations" -ForegroundColor Magenta
$migrationsPath = "Migrations"
if (Test-Path $migrationsPath) {
    $migrations = Get-ChildItem $migrationsPath -Filter "*.cs" | Measure-Object
    Write-Host "✅ $($migrations.Count) fichiers de migration trouvés" -ForegroundColor Green
} else {
    Write-Host "⚠️  Aucun dossier de migration trouvé" -ForegroundColor Yellow
    Write-Host "🔧 Création de la migration initiale..." -ForegroundColor Yellow
    if (-not (Invoke-DotnetCommand "ef migrations add InitialCreate --project ../Catalog.Infrastructure --startup-project ." "Création migration initiale")) {
        Write-Host "❌ Échec de création de la migration" -ForegroundColor Red
        exit 1
    }
}

# Étape 3: Reset de la base de données si demandé
if ($Reset) {
    Write-Host "`n🗑️  ÉTAPE 3: Reset de la base de données" -ForegroundColor Magenta
    Write-Host "⚠️  ATTENTION: Ceci va supprimer toutes les données!" -ForegroundColor Red
    
    if ($Force) {
        $confirm = "y"
    } else {
        $confirm = Read-Host "Êtes-vous sûr de vouloir reset la base? (y/N)"
    }
    
    if ($confirm -eq "y" -or $confirm -eq "Y") {
        Invoke-DotnetCommand "ef database drop --force" "Suppression de la base existante"
    } else {
        Write-Host "⏭️  Reset annulé" -ForegroundColor Yellow
    }
}

# Étape 4: Application des migrations
Write-Host "`n⚡ ÉTAPE 4: Application des migrations" -ForegroundColor Magenta
if (-not (Invoke-DotnetCommand "ef database update --project ../Catalog.Infrastructure --startup-project ." "Application des migrations")) {
    Write-Host "❌ Échec de l'application des migrations" -ForegroundColor Red
    
    # Diagnostic
    Write-Host "`n🔍 DIAGNOSTIC:" -ForegroundColor Yellow
    Write-Host "1. Vérifiez que MySQL est démarré" -ForegroundColor Cyan
    Write-Host "2. Vérifiez la chaîne de connexion dans appsettings.json" -ForegroundColor Cyan
    Write-Host "3. Vérifiez que la base 'niespro_catalog_dev' peut être créée" -ForegroundColor Cyan
    
    exit 1
}

# Étape 5: Vérification de la base créée
Write-Host "`n✅ ÉTAPE 5: Vérification de la base créée" -ForegroundColor Magenta

# Utiliser notre script d'inspection
$inspectorPath = "C:\Users\HP\Documents\projets\NiesPro\tools\catalog-db-inspector.ps1"
if (Test-Path $inspectorPath) {
    Write-Host "🔍 Test de connexion..." -ForegroundColor Yellow
    & $inspectorPath -Action "test"
    
    Write-Host "📋 Tables créées..." -ForegroundColor Yellow  
    & $inspectorPath -Action "tables"
} else {
    Write-Host "⚠️  Script d'inspection non trouvé, vérification manuelle recommandée" -ForegroundColor Yellow
}

# Étape 6: Initialisation des données (seed) si demandé
if ($Seed) {
    Write-Host "`n🌱 ÉTAPE 6: Initialisation des données de test" -ForegroundColor Magenta
    
    # Les données de seed sont normalement dans les migrations ou via DbContext
    Write-Host "ℹ️  Les données initiales sont appliquées via les migrations" -ForegroundColor Cyan
    Write-Host "📊 Vérification des données..." -ForegroundColor Yellow
    
    if (Test-Path $inspectorPath) {
        & $inspectorPath -Action "counts"
    }
}

# Étape 7: Résumé final
Write-Host "`n🎉 SETUP TERMINÉ!" -ForegroundColor Green
Write-Host "=================" -ForegroundColor Green
Write-Host "Base de données: niespro_catalog_dev" -ForegroundColor Cyan
Write-Host "Environnement: $Environment" -ForegroundColor Cyan
Write-Host ""
Write-Host "🔧 Commandes utiles:" -ForegroundColor Yellow
Write-Host "- Inspecter la base: tools\catalog-db-inspector.ps1" -ForegroundColor White
Write-Host "- Démarrer le service: dotnet run (dans $catalogPath)" -ForegroundColor White
Write-Host "- Swagger UI: https://localhost:5013/swagger" -ForegroundColor White
Write-Host ""
Write-Host "📊 Pour vérifier les données:" -ForegroundColor Yellow
Write-Host "tools\catalog-db-inspector.ps1 -Action counts" -ForegroundColor White