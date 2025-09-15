# ===============================================
# Script de Test de Compilation - NiesPro
# ===============================================
# Description: Test de compilation de tous les microservices
# Auteur: NiesPro Development Team
# Date: $(Get-Date -Format "dd/MM/yyyy")
# ===============================================

param(
    [switch]$Verbose,
    [switch]$CleanBuild,
    [string]$Service = "all"
)

# Configuration
$projectRoot = Split-Path $PSScriptRoot -Parent
$services = @("Auth", "Payment", "Order", "Catalog")
$buildResults = @{}

# Couleurs pour l'affichage
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Success($message) { Write-ColorOutput Green $message }
function Write-Error($message) { Write-ColorOutput Red $message }
function Write-Warning($message) { Write-ColorOutput Yellow $message }
function Write-Info($message) { Write-ColorOutput Cyan $message }

# Fonction pour afficher le header
function Show-Header {
    Clear-Host
    Write-Info "==============================================="
    Write-Info "🧪 TESTS DE COMPILATION - NIESPRO"
    Write-Info "==============================================="
    Write-Info "Démarrage des tests: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"
    Write-Info "Répertoire projet: $projectRoot"
    Write-Info "==============================================="
    Write-Output ""
}

# Fonction pour tester un service
function Test-ServiceCompilation($serviceName) {
    Write-Info "🔧 Test du service: $serviceName"
    
    $servicePath = Join-Path $projectRoot "src\Services\$serviceName"
    
    if (-not (Test-Path $servicePath)) {
        Write-Error "❌ Service $serviceName introuvable à: $servicePath"
        return $false
    }
    
    # Test de chaque couche du service
    $layers = @("Domain", "Application", "Infrastructure", "API")
    $serviceResults = @{}
    
    foreach ($layer in $layers) {
        $layerPath = Join-Path $servicePath "$serviceName.$layer"
        
        if (Test-Path $layerPath) {
            Write-Output "  📦 Test $serviceName.$layer..."
            
            Push-Location $layerPath
            
            if ($CleanBuild) {
                dotnet clean --verbosity quiet | Out-Null
            }
            
            $buildOutput = dotnet build --no-restore --verbosity quiet 2>&1
            $buildSuccess = $LASTEXITCODE -eq 0
            
            Pop-Location
            
            if ($buildSuccess) {
                Write-Success "  ✅ $serviceName.$layer: SUCCÈS"
                $serviceResults[$layer] = "SUCCESS"
            } else {
                Write-Error "  ❌ $serviceName.$layer: ÉCHEC"
                if ($Verbose) {
                    Write-Output "    Détails: $buildOutput"
                }
                $serviceResults[$layer] = "FAILED"
            }
        } else {
            Write-Warning "  ⚠️  $serviceName.$layer: NON TROUVÉ"
            $serviceResults[$layer] = "NOT_FOUND"
        }
    }
    
    $buildResults[$serviceName] = $serviceResults
    return $serviceResults.Values -notcontains "FAILED"
}

# Fonction pour tester BuildingBlocks
function Test-BuildingBlocks {
    Write-Info "🏗️  Test des BuildingBlocks"
    
    $buildingBlocksPath = Join-Path $projectRoot "src\BuildingBlocks"
    $blocks = @("Common", "Infrastructure", "WebApi", "Contracts")
    $success = $true
    
    foreach ($block in $blocks) {
        $blockPath = Join-Path $buildingBlocksPath $block
        
        if (Test-Path $blockPath) {
            Write-Output "  📦 Test BuildingBlocks.$block..."
            
            Push-Location $blockPath
            $buildOutput = dotnet build --no-restore --verbosity quiet 2>&1
            $buildSuccess = $LASTEXITCODE -eq 0
            Pop-Location
            
            if ($buildSuccess) {
                Write-Success "  ✅ BuildingBlocks.$block: SUCCÈS"
            } else {
                Write-Error "  ❌ BuildingBlocks.$block: ÉCHEC"
                $success = $false
            }
        }
    }
    
    return $success
}

# Fonction pour afficher le résumé
function Show-Summary {
    Write-Output ""
    Write-Info "==============================================="
    Write-Info "📊 RÉSUMÉ DES TESTS DE COMPILATION"
    Write-Info "==============================================="
    
    $totalServices = 0
    $successfulServices = 0
    
    foreach ($service in $buildResults.Keys) {
        Write-Output ""
        Write-Info "🔧 Service: $service"
        
        foreach ($layer in $buildResults[$service].Keys) {
            $status = $buildResults[$service][$layer]
            $totalServices++
            
            switch ($status) {
                "SUCCESS" { 
                    Write-Success "  ✅ $layer"
                    $successfulServices++
                }
                "FAILED" { Write-Error "  ❌ $layer" }
                "NOT_FOUND" { Write-Warning "  ⚠️  $layer (non trouvé)" }
            }
        }
    }
    
    Write-Output ""
    Write-Info "==============================================="
    $successRate = if ($totalServices -gt 0) { [math]::Round(($successfulServices / $totalServices) * 100, 1) } else { 0 }
    
    if ($successRate -eq 100) {
        Write-Success "🎉 TOUS LES TESTS RÉUSSIS: $successfulServices/$totalServices ($successRate%)"
    } elseif ($successRate -ge 80) {
        Write-Warning "⚠️  MAJORITAIREMENT RÉUSSI: $successfulServices/$totalServices ($successRate%)"
    } else {
        Write-Error "❌ ÉCHECS CRITIQUES: $successfulServices/$totalServices ($successRate%)"
    }
    
    Write-Info "Fin des tests: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"
    Write-Info "==============================================="
}

# SCRIPT PRINCIPAL
# ===============================================

try {
    Show-Header
    
    # Test des BuildingBlocks d'abord
    $buildingBlocksSuccess = Test-BuildingBlocks
    Write-Output ""
    
    # Test des services
    if ($Service -eq "all") {
        foreach ($serviceName in $services) {
            Test-ServiceCompilation $serviceName | Out-Null
            Write-Output ""
        }
    } else {
        if ($services -contains $Service) {
            Test-ServiceCompilation $Service | Out-Null
        } else {
            Write-Error "Service '$Service' non reconnu. Services disponibles: $($services -join ', ')"
            exit 1
        }
    }
    
    # Affichage du résumé
    Show-Summary
    
    # Test global final
    Write-Output ""
    Write-Info "🔄 Test de compilation globale..."
    Push-Location $projectRoot
    $globalBuild = dotnet build --no-restore --verbosity quiet 2>&1
    $globalSuccess = $LASTEXITCODE -eq 0
    Pop-Location
    
    if ($globalSuccess) {
        Write-Success "🎉 COMPILATION GLOBALE: SUCCÈS"
        Write-Success "✅ Projet NiesPro prêt pour les tests fonctionnels!"
    } else {
        Write-Error "❌ COMPILATION GLOBALE: ÉCHEC"
        Write-Error "⚠️  Des problèmes subsistent dans le projet"
    }
    
} catch {
    Write-Error "❌ Erreur durant les tests: $($_.Exception.Message)"
    exit 1
}

Write-Output ""
Write-Info "Tests terminés. Appuyez sur une touche pour continuer..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")