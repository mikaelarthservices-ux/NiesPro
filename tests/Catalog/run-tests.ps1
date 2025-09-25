# Script de test complet pour Catalog.API
param(
    [string]$TestType = "all",
    [switch]$Coverage = $false,
    [switch]$Verbose = $false
)

Write-Host "🧪 CATALOG.API - SUITE DE TESTS COMPLÈTE" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"
$testResults = @()
$totalTests = 0
$passedTests = 0
$failedTests = 0

# Configuration des chemins
$rootPath = Split-Path -Parent -Path $PSScriptRoot
$catalogRoot = Join-Path $rootPath "src\Services\Catalog"
$testsRoot = Join-Path $rootPath "tests\Catalog"

function Write-TestHeader {
    param([string]$Title)
    Write-Host ""
    Write-Host "🔍 $Title" -ForegroundColor Yellow
    Write-Host ("=" * 50) -ForegroundColor Yellow
}

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Details = ""
    )
    
    $icon = if ($Success) { "✅" } else { "❌" }
    $color = if ($Success) { "Green" } else { "Red" }
    
    Write-Host "   $icon $TestName" -ForegroundColor $color
    if ($Details) {
        Write-Host "      $Details" -ForegroundColor Gray
    }
    
    if ($Success) {
        $script:passedTests++
    } else {
        $script:failedTests++
    }
    $script:totalTests++
}

# Test 1: Compilation et build
if ($TestType -eq "all" -or $TestType -eq "build") {
    Write-TestHeader "COMPILATION ET BUILD"
    
    try {
        Write-Host "   🔨 Restauration des packages..." -ForegroundColor Gray
        dotnet restore $catalogRoot --verbosity minimal
        
        Write-Host "   🔨 Compilation Catalog.API..." -ForegroundColor Gray
        dotnet build $catalogRoot --no-restore --verbosity minimal
        
        Write-Host "   🔨 Compilation tests unitaires..." -ForegroundColor Gray
        dotnet build "$testsRoot\Unit" --no-restore --verbosity minimal
        
        Write-Host "   🔨 Compilation tests d'intégration..." -ForegroundColor Gray
        dotnet build "$testsRoot\Integration" --no-restore --verbosity minimal
        
        Write-TestResult "Compilation réussie" $true "Tous les projets compilent sans erreur"
    }
    catch {
        Write-TestResult "Erreur de compilation" $false $_.Exception.Message
    }
}

# Test 2: Tests unitaires
if ($TestType -eq "all" -or $TestType -eq "unit") {
    Write-TestHeader "TESTS UNITAIRES"
    
    try {
        $unitTestPath = Join-Path $testsRoot "Unit"
        if (Test-Path $unitTestPath) {
            $verbosityLevel = if ($Verbose) { "normal" } else { "minimal" }
            
            if ($Coverage) {
                Write-Host "   📊 Exécution avec couverture de code..." -ForegroundColor Gray
                $result = dotnet test $unitTestPath --collect:"XPlat Code Coverage" --verbosity $verbosityLevel --logger "console;verbosity=minimal"
            } else {
                $result = dotnet test $unitTestPath --verbosity $verbosityLevel --logger "console;verbosity=minimal"
            }
            
            if ($LASTEXITCODE -eq 0) {
                Write-TestResult "Tests unitaires" $true "Tous les tests unitaires passent"
            } else {
                Write-TestResult "Tests unitaires" $false "Certains tests unitaires échouent"
            }
        } else {
            Write-TestResult "Tests unitaires" $false "Répertoire des tests unitaires introuvable"
        }
    }
    catch {
        Write-TestResult "Tests unitaires" $false $_.Exception.Message
    }
}

# Test 3: Vérification base de données
if ($TestType -eq "all" -or $TestType -eq "database") {
    Write-TestHeader "TESTS BASE DE DONNÉES"
    
    try {
        Write-Host "   🗄️ Test de connexion MySQL..." -ForegroundColor Gray
        $dbTestResult = & "$rootPath\tools\catalog-db-inspector.ps1" -Action "test" 2>&1
        
        if ($dbTestResult -like "*Connexion réussie*") {
            Write-TestResult "Connexion MySQL" $true "Base niespro_catalog accessible"
            
            Write-Host "   🗄️ Vérification des tables..." -ForegroundColor Gray
            $tablesResult = & "$rootPath\tools\catalog-db-inspector.ps1" -Action "tables" 2>&1
            
            if ($tablesResult -like "*categories*" -and $tablesResult -like "*products*") {
                Write-TestResult "Structure base de données" $true "Tables présentes et correctes"
            } else {
                Write-TestResult "Structure base de données" $false "Tables manquantes ou incorrectes"
            }
        } else {
            Write-TestResult "Connexion MySQL" $false "Impossible de se connecter à la base"
        }
    }
    catch {
        Write-TestResult "Tests base de données" $false $_.Exception.Message
    }
}

# Test 4: Tests d'intégration API
if ($TestType -eq "all" -or $TestType -eq "integration") {
    Write-TestHeader "TESTS D'INTÉGRATION API"
    
    try {
        # Vérifier si le service est en cours d'exécution
        $serviceRunning = $false
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5003/health" -TimeoutSec 5 -UseBasicParsing
            $serviceRunning = $response.StatusCode -eq 200
        } catch {
            $serviceRunning = $false
        }
        
        if (-not $serviceRunning) {
            Write-Host "   🚀 Démarrage du service Catalog.API..." -ForegroundColor Gray
            $serviceProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "$catalogRoot\Catalog.API" -WindowStyle Hidden -PassThru
            
            # Attendre que le service démarre
            $attempts = 0
            do {
                Start-Sleep -Seconds 2
                try {
                    $response = Invoke-WebRequest -Uri "http://localhost:5003/health" -TimeoutSec 5 -UseBasicParsing
                    $serviceRunning = $response.StatusCode -eq 200
                } catch {
                    $serviceRunning = $false
                }
                $attempts++
            } while (-not $serviceRunning -and $attempts -lt 15)
            
            if (-not $serviceRunning) {
                Write-TestResult "Démarrage service" $false "Service ne démarre pas dans les temps"
                return
            }
        }
        
        Write-TestResult "Service disponible" $true "API accessible sur port 5003"
        
        # Exécuter tests d'intégration
        $integrationTestPath = Join-Path $testsRoot "Integration"
        if (Test-Path $integrationTestPath) {
            $verbosityLevel = if ($Verbose) { "normal" } else { "minimal" }
            $result = dotnet test $integrationTestPath --verbosity $verbosityLevel --logger "console;verbosity=minimal"
            
            if ($LASTEXITCODE -eq 0) {
                Write-TestResult "Tests d'intégration" $true "Tous les tests d'intégration passent"
            } else {
                Write-TestResult "Tests d'intégration" $false "Certains tests d'intégration échouent"
            }
        } else {
            Write-TestResult "Tests d'intégration" $false "Répertoire des tests d'intégration introuvable"
        }
        
        # Nettoyer le processus si on l'a démarré
        if ($serviceProcess) {
            Stop-Process -Id $serviceProcess.Id -Force -ErrorAction SilentlyContinue
        }
    }
    catch {
        Write-TestResult "Tests d'intégration" $false $_.Exception.Message
    }
}

# Test 5: Tests de performance de base
if ($TestType -eq "all" -or $TestType -eq "performance") {
    Write-TestHeader "TESTS DE PERFORMANCE"
    
    try {
        Write-Host "   ⚡ Exécution du testeur de service..." -ForegroundColor Gray
        $perfResult = & "$rootPath\tools\catalog-service-tester.ps1" 2>&1
        
        if ($perfResult -like "*Taux de succès*") {
            Write-TestResult "Tests de performance" $true "Service répond dans les temps acceptables"
        } else {
            Write-TestResult "Tests de performance" $false "Performance insuffisante ou service indisponible"
        }
    }
    catch {
        Write-TestResult "Tests de performance" $false $_.Exception.Message
    }
}

# Rapport final
Write-Host ""
Write-Host "📊 RAPPORT FINAL DES TESTS" -ForegroundColor Magenta
Write-Host "===========================" -ForegroundColor Magenta
Write-Host "Total des tests    : $totalTests" -ForegroundColor White
Write-Host "Tests réussis      : $passedTests" -ForegroundColor Green
Write-Host "Tests échoués      : $failedTests" -ForegroundColor Red

if ($totalTests -gt 0) {
    $successRate = [math]::Round(($passedTests / $totalTests) * 100, 1)
    Write-Host "Taux de réussite   : $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } else { "Yellow" })
}

# Recommandations
Write-Host ""
if ($failedTests -eq 0) {
    Write-Host "🎉 Tous les tests passent ! Le service Catalog.API est prêt." -ForegroundColor Green
} elseif ($failedTests -le 2) {
    Write-Host "⚠️  Quelques tests échouent. Vérifiez les détails ci-dessus." -ForegroundColor Yellow
} else {
    Write-Host "❌ Plusieurs tests échouent. Action corrective requise." -ForegroundColor Red
}

# Génération du rapport de couverture
if ($Coverage) {
    Write-Host ""
    Write-Host "📊 GÉNÉRATION DU RAPPORT DE COUVERTURE" -ForegroundColor Cyan
    try {
        # Chercher le fichier de couverture le plus récent
        $coverageFile = Get-ChildItem -Path "." -Recurse -Filter "coverage.cobertura.xml" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        
        if ($coverageFile) {
            Write-Host "   ✅ Rapport de couverture généré : $($coverageFile.FullName)" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  Fichier de couverture non trouvé" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ❌ Erreur lors de la génération du rapport de couverture" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Tests terminés à $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray

# Code de sortie basé sur les résultats
if ($failedTests -eq 0) {
    exit 0
} else {
    exit 1
}