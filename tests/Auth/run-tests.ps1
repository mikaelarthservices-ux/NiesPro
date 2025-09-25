# ========================================
# 🧪 AUTH SERVICE - TEST RUNNER
# ========================================
# Script d'exécution automatisée des tests Auth
# Incluant tests unitaires, intégration et rapports

param(
    [Parameter(Position=0)]
    [ValidateSet("all", "unit", "integration", "coverage")]
    [string]$TestType = "all",
    
    [switch]$GenerateCoverage,
    [switch]$GenerateReport,
    [switch]$Verbose,
    [switch]$WatchMode
)

# Configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$AUTH_ROOT = "c:\Users\HP\Documents\projets\NiesPro"
$TESTS_ROOT = "$AUTH_ROOT\tests\Auth"
$UNIT_PROJECT = "$TESTS_ROOT\Unit\Auth.Tests.Unit.csproj"
$INTEGRATION_PROJECT = "$TESTS_ROOT\Integration\Auth.Tests.Integration.csproj"

# Couleurs pour output
$ColorSuccess = "Green"
$ColorWarning = "Yellow" 
$ColorError = "Red"
$ColorInfo = "Cyan"
$ColorHeader = "Magenta"

function Write-Header {
    param([string]$Title)
    Write-Host "`n🧪 $Title" -ForegroundColor $ColorHeader
    Write-Host ("=" * ($Title.Length + 3)) -ForegroundColor $ColorHeader
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $ColorSuccess
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️ $Message" -ForegroundColor $ColorWarning
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $ColorError
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️ $Message" -ForegroundColor $ColorInfo
}

function Test-Prerequisites {
    Write-Header "Vérification des prérequis"
    
    # Vérifier .NET CLI
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET CLI détecté : v$dotnetVersion"
    }
    catch {
        Write-Error ".NET CLI non trouvé. Veuillez installer .NET 8.0+"
        exit 1
    }
    
    # Vérifier les projets de test
    if (-not (Test-Path $UNIT_PROJECT)) {
        Write-Error "Projet de tests unitaires non trouvé : $UNIT_PROJECT"
        exit 1
    }
    Write-Success "Projet de tests unitaires trouvé"
    
    if (-not (Test-Path $INTEGRATION_PROJECT)) {
        Write-Warning "Projet de tests d'intégration non trouvé : $INTEGRATION_PROJECT"
        Write-Info "Les tests d'intégration seront ignorés"
    } else {
        Write-Success "Projet de tests d'intégration trouvé"
    }
}

function Run-UnitTests {
    Write-Header "Exécution des tests unitaires"
    
    $startTime = Get-Date
    
    try {
        $verbosityLevel = if ($Verbose) { "normal" } else { "minimal" }
        
        Write-Info "Compilation du projet de tests unitaires..."
        dotnet build $UNIT_PROJECT --configuration Release --verbosity $verbosityLevel
        
        Write-Info "Exécution des tests unitaires..."
        if ($GenerateCoverage) {
            dotnet test $UNIT_PROJECT `
                --configuration Release `
                --no-build `
                --verbosity $verbosityLevel `
                --collect:"XPlat Code Coverage" `
                --results-directory "$TESTS_ROOT\TestResults"
        } else {
            dotnet test $UNIT_PROJECT `
                --configuration Release `
                --no-build `
                --verbosity $verbosityLevel
        }
        
        $duration = (Get-Date) - $startTime
        Write-Success "Tests unitaires terminés en $($duration.TotalSeconds.ToString('F1'))s"
        return $true
    }
    catch {
        $duration = (Get-Date) - $startTime
        Write-Error "Échec des tests unitaires après $($duration.TotalSeconds.ToString('F1'))s"
        Write-Error $_.Exception.Message
        return $false
    }
}

function Run-IntegrationTests {
    Write-Header "Exécution des tests d'intégration"
    
    if (-not (Test-Path $INTEGRATION_PROJECT)) {
        Write-Warning "Tests d'intégration non disponibles"
        return $true
    }
    
    $startTime = Get-Date
    
    try {
        $verbosityLevel = if ($Verbose) { "normal" } else { "minimal" }
        
        Write-Info "Compilation du projet de tests d'intégration..."
        dotnet build $INTEGRATION_PROJECT --configuration Release --verbosity $verbosityLevel
        
        Write-Info "Exécution des tests d'intégration..."
        dotnet test $INTEGRATION_PROJECT `
            --configuration Release `
            --no-build `
            --verbosity $verbosityLevel
        
        $duration = (Get-Date) - $startTime
        Write-Success "Tests d'intégration terminés en $($duration.TotalSeconds.ToString('F1'))s"
        return $true
    }
    catch {
        $duration = (Get-Date) - $startTime  
        Write-Error "Échec des tests d'intégration après $($duration.TotalSeconds.ToString('F1'))s"
        Write-Error $_.Exception.Message
        return $false
    }
}

function Generate-CoverageReport {
    Write-Header "Génération du rapport de couverture"
    
    $coverageFiles = Get-ChildItem "$TESTS_ROOT\TestResults" -Recurse -Filter "coverage.cobertura.xml"
    
    if ($coverageFiles.Count -eq 0) {
        Write-Warning "Aucun fichier de couverture trouvé"
        return
    }
    
    try {
        # Installer reportgenerator si nécessaire
        if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
            Write-Info "Installation de ReportGenerator..."
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }
        
        $latestCoverage = $coverageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        $reportDir = "$TESTS_ROOT\CoverageReport"
        
        Write-Info "Génération du rapport HTML..."
        reportgenerator `
            -reports:$($latestCoverage.FullName) `
            -targetdir:$reportDir `
            -reporttypes:"Html;TextSummary"
        
        Write-Success "Rapport de couverture généré : $reportDir\index.html"
        
        if (Test-Path "$reportDir\Summary.txt") {
            Write-Info "Résumé de couverture :"
            Get-Content "$reportDir\Summary.txt" | Write-Host
        }
    }
    catch {
        Write-Error "Erreur lors de la génération du rapport : $($_.Exception.Message)"
    }
}

function Generate-TestReport {
    Write-Header "Génération du rapport de test"
    
    $reportFile = "$TESTS_ROOT\Test-Report-$(Get-Date -Format 'yyyy-MM-dd').md"
    
    $report = @"
# Auth Service - Rapport de Test
*Généré automatiquement le $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')*

## 📊 Résumé Exécutif

- **Service** : Auth.API
- **Date d'exécution** : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
- **Environnement** : Test
- **Machine** : $env:COMPUTERNAME

## 🧪 Résultats des Tests

### Tests Unitaires
- **Status** : $(if ($unitTestsSuccess) { "✅ SUCCÈS" } else { "❌ ÉCHEC" })
- **Projet** : Auth.Tests.Unit.csproj

### Tests d'Intégration  
- **Status** : $(if (Test-Path $INTEGRATION_PROJECT) { if ($integrationTestsSuccess) { "✅ SUCCÈS" } else { "❌ ÉCHEC" } } else { "🚧 NON DISPONIBLE" })
- **Projet** : Auth.Tests.Integration.csproj

## 📈 Métriques

### Performance
- **Tests unitaires** : $unitTestDuration secondes
- **Tests d'intégration** : $integrationTestDuration secondes

### Qualité
- **Couverture de code** : $(if ($GenerateCoverage) { "Générée" } else { "Non calculée" })

## 📋 Actions Recommandées

$(if (-not $unitTestsSuccess) { "- ❌ Corriger les tests unitaires en échec" } else { "- ✅ Tests unitaires validés" })
$(if ((Test-Path $INTEGRATION_PROJECT) -and (-not $integrationTestsSuccess)) { "- ❌ Corriger les tests d'intégration en échec" } elseif (-not (Test-Path $INTEGRATION_PROJECT)) { "- 🚧 Implémenter les tests d'intégration" } else { "- ✅ Tests d'intégration validés" })

---
*Rapport généré par run-tests.ps1*
"@

    $report | Out-File -FilePath $reportFile -Encoding UTF8
    Write-Success "Rapport généré : $reportFile"
}

function Show-Summary {
    Write-Header "Résumé d'exécution"
    
    $totalDuration = (Get-Date) - $globalStartTime
    
    Write-Info "Temps total d'exécution : $($totalDuration.TotalSeconds.ToString('F1'))s"
    
    if ($unitTestsSuccess) {
        Write-Success "Tests unitaires : SUCCÈS"
    } else {
        Write-Error "Tests unitaires : ÉCHEC"
    }
    
    if (Test-Path $INTEGRATION_PROJECT) {
        if ($integrationTestsSuccess) {
            Write-Success "Tests d'intégration : SUCCÈS"
        } else {
            Write-Error "Tests d'intégration : ÉCHEC"
        }
    } else {
        Write-Warning "Tests d'intégration : NON DISPONIBLES"
    }
    
    $overallSuccess = $unitTestsSuccess -and ($integrationTestsSuccess -or (-not (Test-Path $INTEGRATION_PROJECT)))
    
    if ($overallSuccess) {
        Write-Host "`n🎉 TOUS LES TESTS SONT PASSÉS AVEC SUCCÈS ! 🎯" -ForegroundColor Green -BackgroundColor Black
    } else {
        Write-Host "`n💥 CERTAINS TESTS ONT ÉCHOUÉ" -ForegroundColor Red -BackgroundColor Black
        exit 1
    }
}

# ========================================
# EXÉCUTION PRINCIPALE
# ========================================

$globalStartTime = Get-Date

Write-Host @"
🧪 AUTH SERVICE TEST RUNNER
============================
Type de test : $TestType
Couverture : $(if ($GenerateCoverage) { "Activée" } else { "Désactivée" })
Rapport : $(if ($GenerateReport) { "Activé" } else { "Désactivé" })
Mode verbose : $(if ($Verbose) { "Activé" } else { "Désactivé" })
"@ -ForegroundColor $ColorHeader

# Vérification des prérequis
Test-Prerequisites

# Variables pour tracking
$unitTestsSuccess = $false
$integrationTestsSuccess = $false
$unitTestDuration = 0
$integrationTestDuration = 0

# Exécution des tests selon le type demandé
switch ($TestType.ToLower()) {
    "unit" {
        $unitStart = Get-Date
        $unitTestsSuccess = Run-UnitTests
        $unitTestDuration = ((Get-Date) - $unitStart).TotalSeconds
        
        if ($GenerateCoverage) {
            Generate-CoverageReport
        }
    }
    
    "integration" {
        $integrationStart = Get-Date  
        $integrationTestsSuccess = Run-IntegrationTests
        $integrationTestDuration = ((Get-Date) - $integrationStart).TotalSeconds
    }
    
    "coverage" {
        $GenerateCoverage = $true
        $unitStart = Get-Date
        $unitTestsSuccess = Run-UnitTests  
        $unitTestDuration = ((Get-Date) - $unitStart).TotalSeconds
        
        Generate-CoverageReport
    }
    
    "all" {
        $unitStart = Get-Date
        $unitTestsSuccess = Run-UnitTests
        $unitTestDuration = ((Get-Date) - $unitStart).TotalSeconds
        
        $integrationStart = Get-Date
        $integrationTestsSuccess = Run-IntegrationTests  
        $integrationTestDuration = ((Get-Date) - $integrationStart).TotalSeconds
        
        if ($GenerateCoverage) {
            Generate-CoverageReport
        }
    }
}

# Génération du rapport si demandé
if ($GenerateReport) {
    Generate-TestReport
}

# Affichage du résumé
Show-Summary