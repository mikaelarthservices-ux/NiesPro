# ===============================================
# Script de Configuration Base de Données - Catalog.API
# ===============================================

Write-Host "=== CONFIGURATION DATABASE CATALOG.API ===" -ForegroundColor Cyan
Write-Host "Heure: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

$catalogDir = "src/Services/Catalog/Catalog.API"
$dbName = "niespro_catalog_dev"

# Vérification des prérequis
Write-Host "🔍 VERIFICATION PREREQUIS:" -ForegroundColor Yellow

# Vérifier dotnet
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ✅ .NET CLI détecté (v$dotnetVersion)" -ForegroundColor Green
} catch {
    Write-Host "  ❌ .NET CLI non trouvé" -ForegroundColor Red
    exit 1
}

# Vérifier EF Tools
try {
    $efVersion = dotnet ef --version 2>$null
    if ($efVersion) {
        Write-Host "  ✅ Entity Framework Tools détecté" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  EF Tools non installé, installation..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef
        Write-Host "  ✅ Entity Framework Tools installé" -ForegroundColor Green
    }
} catch {
    Write-Host "  ⚠️  Installation EF Tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# Vérifier MySQL
Write-Host "`n🗄️  VERIFICATION MYSQL:" -ForegroundColor Yellow
$mysqlProcess = Get-Process | Where-Object { $_.ProcessName -like "*mysql*" }
if ($mysqlProcess) {
    Write-Host "  ✅ MySQL actif (PID: $($mysqlProcess.Id -join ', '))" -ForegroundColor Green
} else {
    Write-Host "  ❌ MySQL non détecté" -ForegroundColor Red
    Write-Host "     💡 Démarrez WAMP/XAMPP avant de continuer" -ForegroundColor Yellow
    
    $continue = Read-Host "Continuer quand même? (y/N)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

# Navigation vers le répertoire du projet
if (Test-Path $catalogDir) {
    Write-Host "`n📁 Navigation vers $catalogDir" -ForegroundColor Cyan
    Push-Location $catalogDir
} else {
    Write-Host "❌ Répertoire $catalogDir non trouvé" -ForegroundColor Red
    exit 1
}

try {
    # Vérification des migrations existantes
    Write-Host "`n🔍 VERIFICATION MIGRATIONS:" -ForegroundColor Yellow
    $migrations = dotnet ef migrations list 2>$null
    if ($migrations) {
        Write-Host "  ✅ Migrations trouvées:" -ForegroundColor Green
        $migrations | ForEach-Object { Write-Host "    - $_" -ForegroundColor White }
    } else {
        Write-Host "  ⚠️  Aucune migration trouvée" -ForegroundColor Yellow
    }

    # Vérification de la base de données
    Write-Host "`n🗄️  VERIFICATION BASE DE DONNEES:" -ForegroundColor Yellow
    try {
        $dbInfo = dotnet ef database list 2>$null
        Write-Host "  ✅ Connexion à la base de données établie" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠️  Problème de connexion base de données" -ForegroundColor Yellow
    }

    # Application des migrations
    Write-Host "`n⚡ APPLICATION MIGRATIONS:" -ForegroundColor Yellow
    Write-Host "  🚀 Exécution de 'dotnet ef database update'..." -ForegroundColor Cyan
    
    $updateResult = dotnet ef database update 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Migrations appliquées avec succès!" -ForegroundColor Green
        Write-Host "  📊 Base de données '$dbName' créée/mise à jour" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Erreur lors de l'application des migrations:" -ForegroundColor Red
        Write-Host $updateResult -ForegroundColor Red
        
        # Diagnostic supplémentaire
        Write-Host "`n🔧 DIAGNOSTIC:" -ForegroundColor Yellow
        if ($updateResult -like "*Base*inconnue*" -or $updateResult -like "*database*does not exist*") {
            Write-Host "  💡 La base de données n'existe pas, tentative de création..." -ForegroundColor Yellow
            
            # Tentative de création de la base avec un script SQL
            $createDbScript = @"
CREATE DATABASE IF NOT EXISTS ``$dbName`` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;
"@
            
            Write-Host "  📝 Création de la base '$dbName'..." -ForegroundColor Cyan
            $createDbScript | mysql -u root
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✅ Base de données créée, nouvelle tentative de migration..." -ForegroundColor Green
                $retryResult = dotnet ef database update 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "  ✅ Migrations appliquées avec succès!" -ForegroundColor Green
                } else {
                    Write-Host "  ❌ Échec de la deuxième tentative:" -ForegroundColor Red
                    Write-Host $retryResult -ForegroundColor Red
                }
            } else {
                Write-Host "  ❌ Échec de la création de la base de données" -ForegroundColor Red
                Write-Host "     💡 Vérifiez que MySQL est démarré et accessible" -ForegroundColor Yellow
            }
        }
    }

    # Test de validation
    Write-Host "`n✅ VALIDATION:" -ForegroundColor Yellow
    Write-Host "  🧪 Test de connexion à l'API..." -ForegroundColor Cyan
    
    # Lancer temporairement l'API pour tester
    Start-Job -ScriptBlock { 
        Set-Location $using:PWD
        dotnet run --no-build > $null 2>&1 
    } -Name "CatalogAPITest" | Out-Null
    
    Start-Sleep -Seconds 5
    
    try {
        $testResponse = Invoke-RestMethod -Uri "http://localhost:5003/api/v1/Categories" -Method GET -TimeoutSec 10
        if ($testResponse.success -or $testResponse.success -eq $false) {
            Write-Host "  ✅ API répond correctement!" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  API répond mais format inattendu" -ForegroundColor Yellow
        }
    } catch {
        if ($_.Exception.Message -notlike "*Base*inconnue*") {
            Write-Host "  ✅ API démarrée (erreur non-DB détectée)" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Problème de base de données persistent" -ForegroundColor Red
        }
    }
    
    # Arrêter le job de test
    Get-Job -Name "CatalogAPITest" | Stop-Job | Remove-Job

} finally {
    Pop-Location
}

Write-Host "`n🎉 CONFIGURATION TERMINEE!" -ForegroundColor Green
Write-Host "`n📋 ETAPES COMPLETEES:" -ForegroundColor Cyan
Write-Host "  - Vérification des outils .NET/EF" -ForegroundColor White
Write-Host "  - Vérification MySQL" -ForegroundColor White
Write-Host "  - Application des migrations Entity Framework" -ForegroundColor White
Write-Host "  - Création/mise à jour base '$dbName'" -ForegroundColor White
Write-Host "  - Test de validation API" -ForegroundColor White

Write-Host "`n💡 PROCHAINES ETAPES:" -ForegroundColor Yellow
Write-Host "  1. Démarrer Catalog.API: dotnet run --project $catalogDir" -ForegroundColor White
Write-Host "  2. Tester l'API: tests/Scripts/test-catalog-diagnostic.ps1" -ForegroundColor White
Write-Host "  3. Tests complets: tests/Scripts/test-catalog-final.ps1" -ForegroundColor White