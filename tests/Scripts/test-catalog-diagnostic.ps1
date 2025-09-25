# ===============================================
# Script de Diagnostic Rapide - Catalog.API
# ===============================================

Write-Host "=== DIAGNOSTIC CATALOG.API ===" -ForegroundColor Cyan
Write-Host "Heure: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

$baseUrl = "http://localhost:5003"
$httpsUrl = "https://localhost:5013"

# Test du port et du processus
Write-Host "🔍 STATUS PROCESSUS:" -ForegroundColor Yellow
$catalogProcess = Get-Process | Where-Object { $_.ProcessName -eq "Catalog.API" }
if ($catalogProcess) {
    Write-Host "  ✅ Catalog.API actif (PID: $($catalogProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "  ❌ Catalog.API non détecté" -ForegroundColor Red
}

# Test des ports
Write-Host "`n🔍 PORTS EN ECOUTE:" -ForegroundColor Yellow
$port5003 = netstat -ano | findstr ":5003 " | findstr "LISTENING"
$port5013 = netstat -ano | findstr ":5013 " | findstr "LISTENING"

if ($port5003) {
    $pid5003 = ($port5003 -split '\s+')[-1]
    Write-Host "  ✅ HTTP Port 5003 actif - PID: $pid5003" -ForegroundColor Green
} else {
    Write-Host "  ❌ HTTP Port 5003 inactif" -ForegroundColor Red
}

if ($port5013) {
    $pid5013 = ($port5013 -split '\s+')[-1]
    Write-Host "  ✅ HTTPS Port 5013 actif - PID: $pid5013" -ForegroundColor Green
} else {
    Write-Host "  ❌ HTTPS Port 5013 inactif" -ForegroundColor Red
}

# Test de connectivité HTTP
Write-Host "`n🏥 CONNECTIVITE HTTP:" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl" -UseBasicParsing -TimeoutSec 5
    Write-Host "  ✅ HTTP accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "  ❌ HTTP inaccessible: $($_.Exception.Message)" -ForegroundColor Red
}

# Test API Endpoints
Write-Host "`n🔍 TEST ENDPOINTS API:" -ForegroundColor Yellow

# Test Categories
try {
    $categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Categories" -Method GET -TimeoutSec 5
    if ($categoriesResponse.success) {
        $categoryCount = if ($categoriesResponse.data) { $categoriesResponse.data.Count } else { 0 }
        Write-Host "  ✅ GET Categories OK ($categoryCount catégories)" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  GET Categories: $($categoriesResponse.message)" -ForegroundColor Yellow
    }
} catch {
    $errorMsg = $_.Exception.Message
    if ($errorMsg -like "*Base*inconnue*" -or $errorMsg -like "*database*") {
        Write-Host "  ❌ GET Categories: Problème base de données" -ForegroundColor Red
        Write-Host "     💡 Créez la DB avec: scripts/database/create_databases.ps1" -ForegroundColor Yellow
    } else {
        Write-Host "  ❌ GET Categories: $errorMsg" -ForegroundColor Red
    }
}

# Test Products
try {
    $productsResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Products" -Method GET -TimeoutSec 5
    if ($productsResponse.success) {
        $productCount = if ($productsResponse.data -and $productsResponse.data.items) { $productsResponse.data.items.Count } else { 0 }
        Write-Host "  ✅ GET Products OK ($productCount produits)" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  GET Products: $($productsResponse.message)" -ForegroundColor Yellow
    }
} catch {
    $errorMsg = $_.Exception.Message
    if ($errorMsg -like "*Base*inconnue*" -or $errorMsg -like "*database*") {
        Write-Host "  ❌ GET Products: Problème base de données" -ForegroundColor Red
    } else {
        Write-Host "  ❌ GET Products: $errorMsg" -ForegroundColor Red
    }
}

# Test MySQL
Write-Host "`n🗄️  BASE DE DONNEES:" -ForegroundColor Yellow
$mysqlProcess = Get-Process | Where-Object { $_.ProcessName -like "*mysql*" }
if ($mysqlProcess) {
    Write-Host "  ✅ MySQL actif (PID: $($mysqlProcess.Id -join ', '))" -ForegroundColor Green
} else {
    Write-Host "  ❌ MySQL non détecté" -ForegroundColor Red
    Write-Host "     💡 Démarrez WAMP/XAMPP ou MySQL" -ForegroundColor Yellow
}

# Vérification des fichiers de migration
Write-Host "`n📁 MIGRATIONS:" -ForegroundColor Yellow
$migrationDir = "src/Services/Catalog/Catalog.Infrastructure/Migrations"
if (Test-Path $migrationDir) {
    $migrations = Get-ChildItem -Path $migrationDir -Filter "*.cs" | Where-Object { $_.Name -like "*Migration*" }
    Write-Host "  ✅ $($migrations.Count) fichiers de migration trouvés" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Répertoire de migrations non trouvé" -ForegroundColor Yellow
}

Write-Host "`n📋 RESUME:" -ForegroundColor Green
Write-Host "  Service: Catalog.API" -ForegroundColor White
Write-Host "  Ports: 5003 (HTTP), 5013 (HTTPS)" -ForegroundColor White
Write-Host "  Base: niespro_catalog_dev (MySQL)" -ForegroundColor White
Write-Host "  Endpoints: /api/v1/Categories, /api/v1/Products" -ForegroundColor White

Write-Host "`n💡 COMMANDES UTILES:" -ForegroundColor Cyan
Write-Host "  Démarrer: dotnet run --project src/Services/Catalog/Catalog.API" -ForegroundColor White
Write-Host "  Migrations: cd src/Services/Catalog/Catalog.API && dotnet ef database update" -ForegroundColor White
Write-Host "  Tests: tests/Scripts/test-catalog-final.ps1" -ForegroundColor White