# Tests complets Catalog.API - Suite de validation finale
Write-Host "=== TESTS COMPLETS CATALOG.API ===" -ForegroundColor Green

$baseUrl = "http://localhost:5003"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# Données de test pour catégories
$testCategory = @{
    name = "Test Category $timestamp"
    description = "Category de test créée le $(Get-Date -Format 'dd/MM/yyyy HH:mm')"
    slug = "test-category-$timestamp"
    sortOrder = 100
    isActive = $true
}

# Données de test pour produits
$testProduct = @{
    name = "Test Product $timestamp"
    sku = "TEST-SKU-$timestamp"
    description = "Produit de test créé le $(Get-Date -Format 'dd/MM/yyyy HH:mm')"
    price = 99.99
    trackQuantity = $true
    quantity = 50
    isActive = $true
    isFeatured = $false
}

Write-Host "`nTimestamp de test: $timestamp" -ForegroundColor Cyan

# Test 1: Health Check / Service Status
Write-Host "`n🔍 TEST 1: Service Status" -ForegroundColor Yellow
try {
    # Test de base - port ouvert
    $response = Invoke-WebRequest -Uri "$baseUrl" -UseBasicParsing -TimeoutSec 10
    Write-Host "✅ Service accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "❌ Service inaccessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "🔧 Vérifiez que Catalog.API est démarré sur le port 5003" -ForegroundColor Yellow
    exit 1
}

# Test 2: Test de base de données / Migration
Write-Host "`n🔍 TEST 2: Connectivité Base de Données" -ForegroundColor Yellow
try {
    # Test avec GET Categories (basique)
    $categoriesResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Categories" -Method GET -TimeoutSec 10
    if ($categoriesResponse.success -eq $false -and $categoriesResponse.message -like "*database*") {
        Write-Host "❌ Problème de base de données détecté" -ForegroundColor Red
        Write-Host "💡 Suggestion: Exécutez les migrations avec 'dotnet ef database update'" -ForegroundColor Yellow
    } elseif ($categoriesResponse.success) {
        Write-Host "✅ Base de données connectée - $($categoriesResponse.data.Count) catégories trouvées" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Réponse inattendue: $($categoriesResponse.message)" -ForegroundColor Yellow
    }
} catch {
    if ($_.Exception.Message -like "*Base*inconnue*" -or $_.Exception.Message -like "*database*") {
        Write-Host "❌ Base de données 'niespro_catalog_dev' introuvable" -ForegroundColor Red
        Write-Host "💡 Suggestion: Créez la base avec les scripts database/create_databases.ps1" -ForegroundColor Yellow
    } else {
        Write-Host "❌ Erreur de connectivité DB: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 3: CRUD Catégories
Write-Host "`n🔍 TEST 3: CRUD Catégories" -ForegroundColor Yellow

# 3A: Créer une catégorie
Write-Host "  📝 Création de catégorie..." -ForegroundColor Cyan
$createCategoryData = $testCategory | ConvertTo-Json
$createdCategoryId = $null

try {
    $createCategoryResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Categories" -Method POST -Body $createCategoryData -ContentType "application/json"
    if ($createCategoryResponse.success) {
        $createdCategoryId = $createCategoryResponse.data.id
        Write-Host "  ✅ Catégorie créée avec succès! ID: $createdCategoryId" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Échec création catégorie: $($createCategoryResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Erreur création catégorie: $($_.Exception.Message)" -ForegroundColor Red
}

# 3B: Lire la catégorie créée
if ($createdCategoryId) {
    Write-Host "  📖 Lecture de la catégorie créée..." -ForegroundColor Cyan
    try {
        $getCategoryResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Categories/$createdCategoryId" -Method GET
        if ($getCategoryResponse.success -and $getCategoryResponse.data.name -eq $testCategory.name) {
            Write-Host "  ✅ Catégorie récupérée avec succès!" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Données de catégorie incorrectes" -ForegroundColor Red
        }
    } catch {
        Write-Host "  ❌ Erreur lecture catégorie: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 4: CRUD Produits
Write-Host "`n🔍 TEST 4: CRUD Produits" -ForegroundColor Yellow

# 4A: Créer un produit (si on a une catégorie)
if ($createdCategoryId) {
    Write-Host "  📝 Création de produit..." -ForegroundColor Cyan
    $testProduct.categoryId = $createdCategoryId
    $createProductData = $testProduct | ConvertTo-Json
    $createdProductId = $null

    try {
        $createProductResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Products" -Method POST -Body $createProductData -ContentType "application/json"
        if ($createProductResponse.success) {
            $createdProductId = $createProductResponse.data.id
            Write-Host "  ✅ Produit créé avec succès! ID: $createdProductId" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Échec création produit: $($createProductResponse.message)" -ForegroundColor Red
        }
    } catch {
        Write-Host "  ❌ Erreur création produit: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 4B: Lire le produit créé
    if ($createdProductId) {
        Write-Host "  📖 Lecture du produit créé..." -ForegroundColor Cyan
        try {
            $getProductResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Products/$createdProductId" -Method GET
            if ($getProductResponse.success -and $getProductResponse.data.name -eq $testProduct.name) {
                Write-Host "  ✅ Produit récupéré avec succès!" -ForegroundColor Green
            } else {
                Write-Host "  ❌ Données de produit incorrectes" -ForegroundColor Red
            }
        } catch {
            Write-Host "  ❌ Erreur lecture produit: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Test 5: Liste des produits avec filtres
Write-Host "`n🔍 TEST 5: Filtres et Pagination" -ForegroundColor Yellow
try {
    # Test pagination
    $productsPageResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Products?pageNumber=1&pageSize=5" -Method GET
    if ($productsPageResponse.success) {
        Write-Host "  ✅ Pagination fonctionne - Page 1/5 récupérée" -ForegroundColor Green
    }

    # Test filtre par catégorie (si on a une catégorie)
    if ($createdCategoryId) {
        $productsByCategoryResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Products?categoryId=$createdCategoryId" -Method GET
        if ($productsByCategoryResponse.success) {
            Write-Host "  ✅ Filtre par catégorie fonctionne" -ForegroundColor Green
        }
    }
} catch {
    Write-Host "  ❌ Erreur tests filtres: $($_.Exception.Message)" -ForegroundColor Red
}

# Nettoyage - Suppression des données de test
Write-Host "`n🧹 NETTOYAGE: Suppression des données de test" -ForegroundColor Yellow
if ($createdProductId) {
    try {
        $deleteProductResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Products/$createdProductId" -Method DELETE
        if ($deleteProductResponse.success) {
            Write-Host "  ✅ Produit de test supprimé" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️  Échec suppression produit: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

if ($createdCategoryId) {
    try {
        $deleteCategoryResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/Categories/$createdCategoryId" -Method DELETE
        if ($deleteCategoryResponse.success) {
            Write-Host "  ✅ Catégorie de test supprimée" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️  Échec suppression catégorie: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`n🎉 TESTS TERMINÉS - CATALOG.API VALIDATION COMPLETE!" -ForegroundColor Green
Write-Host "📊 Résumé des tests effectués:" -ForegroundColor Cyan
Write-Host "  - Connectivité service" -ForegroundColor White
Write-Host "  - Connectivité base de données" -ForegroundColor White  
Write-Host "  - CRUD Catégories (Create/Read)" -ForegroundColor White
Write-Host "  - CRUD Produits (Create/Read)" -ForegroundColor White
Write-Host "  - Filtres et pagination" -ForegroundColor White
Write-Host "  - Nettoyage des données de test" -ForegroundColor White