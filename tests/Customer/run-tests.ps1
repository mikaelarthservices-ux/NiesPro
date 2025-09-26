# Script d'exécution des tests Customer - NiesPro Enterprise
# Suit les mêmes patterns que Auth et Catalog

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "  CUSTOMER SERVICE TESTS RUNNER  " -ForegroundColor Cyan
Write-Host "  NiesPro Enterprise Standards   " -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# Configuration
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$UnitTestProject = Join-Path $ProjectRoot "Unit\Customer.Tests.Unit.csproj"
$TestResultsDir = Join-Path $ProjectRoot "TestResults"
$ReportsDir = Join-Path $ProjectRoot "Reports"

# Créer les dossiers si nécessaire
New-Item -ItemType Directory -Force -Path $TestResultsDir | Out-Null
New-Item -ItemType Directory -Force -Path $ReportsDir | Out-Null

Write-Host "`n📋 Configuration Tests Customer:" -ForegroundColor Yellow
Write-Host "   Project: $UnitTestProject" -ForegroundColor Gray
Write-Host "   Results: $TestResultsDir" -ForegroundColor Gray
Write-Host "   Reports: $ReportsDir" -ForegroundColor Gray

# Vérifier que le projet de test existe
if (-not (Test-Path $UnitTestProject)) {
    Write-Host "❌ Projet de test non trouvé: $UnitTestProject" -ForegroundColor Red
    exit 1
}

try {
    # Nettoyer les résultats précédents
    Write-Host "`n🧹 Nettoyage des résultats précédents..." -ForegroundColor Yellow
    Get-ChildItem $TestResultsDir -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force

    # Build du projet de test
    Write-Host "`n🔨 Build du projet Customer Tests..." -ForegroundColor Yellow
    dotnet build $UnitTestProject --configuration Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Échec du build du projet de test"
    }

    # Exécution des tests unitaires avec coverage
    Write-Host "`n🧪 Exécution des tests unitaires Customer..." -ForegroundColor Yellow
    $testCommand = @(
        "test", $UnitTestProject,
        "--configuration", "Release",
        "--no-build",
        "--verbosity", "normal",
        "--collect:`"XPlat Code Coverage`"",
        "--results-directory", $TestResultsDir,
        "--logger", "trx"
    )
    
    & dotnet @testCommand
    if ($LASTEXITCODE -ne 0) {
        throw "Échec des tests unitaires"
    }

    # Rechercher les fichiers de résultats
    $trxFiles = Get-ChildItem -Path $TestResultsDir -Filter "*.trx" -Recurse
    $coverageFiles = Get-ChildItem -Path $TestResultsDir -Filter "coverage.cobertura.xml" -Recurse

    Write-Host "`n📊 Résultats des tests:" -ForegroundColor Green
    Write-Host "   TRX Files: $($trxFiles.Count)" -ForegroundColor Gray
    Write-Host "   Coverage Files: $($coverageFiles.Count)" -ForegroundColor Gray

    # Générer le rapport de coverage si disponible
    if ($coverageFiles.Count -gt 0) {
        Write-Host "`n📈 Génération du rapport de coverage..." -ForegroundColor Yellow
        $latestCoverage = $coverageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        
        # Copier le fichier de coverage dans Reports
        $reportFile = Join-Path $ReportsDir "coverage-$(Get-Date -Format 'yyyy-MM-dd-HHmmss').xml"
        Copy-Item $latestCoverage.FullName $reportFile
        Write-Host "   Coverage report: $reportFile" -ForegroundColor Gray
    }

    # Résumé final
    Write-Host "`n✅ Tests Customer exécutés avec succès!" -ForegroundColor Green
    Write-Host "   Voir les résultats dans: $TestResultsDir" -ForegroundColor Gray
    
    if ($trxFiles.Count -gt 0) {
        $latestTrx = $trxFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        Write-Host "   Rapport TRX: $($latestTrx.Name)" -ForegroundColor Gray
    }

} catch {
    Write-Host "`n❌ Erreur lors de l'exécution des tests:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Tests Customer terminés!" -ForegroundColor Cyan