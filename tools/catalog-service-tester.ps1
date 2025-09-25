# =======================================================================
# CATALOG SERVICE TESTER - NiesPro
# Script de test complet pour le service Catalog.API
# =======================================================================

param(
    [string]$BaseUrl = "http://localhost:5003",
    [switch]$Verbose,
    [switch]$StopOnError,
    [int]$Timeout = 30
)

Write-Host "🧪 CATALOG SERVICE TESTER" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green
Write-Host "URL de base: $BaseUrl" -ForegroundColor Cyan
Write-Host "Timeout: $Timeout secondes" -ForegroundColor Cyan
Write-Host ""

$script:TestCount = 0
$script:SuccessCount = 0
$script:FailCount = 0
$script:Results = @()

# Fonction pour exécuter un test
function Invoke-Test {
    param(
        [string]$Name,
        [string]$Method = "GET",
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{"Accept" = "application/json"},
        [int[]]$ExpectedStatusCodes = @(200),
        [switch]$ExpectSuccess = $true
    )
    
    $script:TestCount++
    Write-Host "🔍 Test $script:TestCount : $Name" -ForegroundColor Yellow
    
    $testResult = @{
        Name = $Name
        Method = $Method
        Endpoint = $Endpoint
        Success = $false
        StatusCode = $null
        Response = $null
        Error = $null
        Duration = $null
    }
    
    try {
        $startTime = Get-Date
        $uri = "$BaseUrl$Endpoint"
        
        $params = @{
            Uri = $uri
            Method = $Method
            Headers = $Headers
            TimeoutSec = $Timeout
        }
        
        if ($Body) {
            if ($Body -is [string]) {
                $params.Body = $Body
                $params.ContentType = "application/json"
            } else {
                $params.Body = $Body | ConvertTo-Json -Depth 10
                $params.ContentType = "application/json"
            }
        }
        
        if ($Verbose) {
            Write-Host "   🌐 $Method $uri" -ForegroundColor Gray
            if ($Body) {
                Write-Host "   📦 Body: $($params.Body)" -ForegroundColor Gray
            }
        }
        
        $response = Invoke-WebRequest @params
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalMilliseconds
        
        $testResult.StatusCode = $response.StatusCode
        $testResult.Duration = $duration
        
        # Parse JSON response if possible
        try {
            $testResult.Response = $response.Content | ConvertFrom-Json
        } catch {
            $testResult.Response = $response.Content
        }
        
        # Vérifier le statut code
        if ($response.StatusCode -in $ExpectedStatusCodes) {
            # Vérifier le success si c'est une réponse API NiesPro
            if ($ExpectSuccess -and $testResult.Response -and $testResult.Response.PSObject.Properties.Name -contains "success") {
                if ($testResult.Response.success) {
                    $testResult.Success = $true
                    Write-Host "   ✅ SUCCESS (${duration}ms) - Status: $($response.StatusCode)" -ForegroundColor Green
                } else {
                    Write-Host "   ❌ API returned success=false: $($testResult.Response.message)" -ForegroundColor Red
                }
            } else {
                $testResult.Success = $true
                Write-Host "   ✅ SUCCESS (${duration}ms) - Status: $($response.StatusCode)" -ForegroundColor Green
            }
        } else {
            Write-Host "   ❌ Unexpected status code: $($response.StatusCode)" -ForegroundColor Red
        }
        
        if ($Verbose -and $testResult.Response) {
            Write-Host "   📄 Response: $($testResult.Response | ConvertTo-Json -Compress)" -ForegroundColor Gray
        }
        
    } catch {
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalMilliseconds
        $testResult.Duration = $duration
        $testResult.Error = $_.Exception.Message
        
        Write-Host "   ❌ ERROR (${duration}ms): $($_.Exception.Message)" -ForegroundColor Red
        
        # Si c'est une erreur HTTP, récupérer plus de détails
        if ($_.Exception -is [Microsoft.PowerShell.Commands.HttpResponseException]) {
            $testResult.StatusCode = $_.Exception.Response.StatusCode.value__
            Write-Host "   🔍 HTTP Status: $($testResult.StatusCode)" -ForegroundColor Gray
        }
    }
    
    $script:Results += $testResult
    
    if ($testResult.Success) {
        $script:SuccessCount++
    } else {
        $script:FailCount++
        if ($StopOnError) {
            Write-Host "⏹️  Arrêt sur erreur demandé" -ForegroundColor Red
            Show-Summary
            exit 1
        }
    }
    
    return $testResult
}

# Fonction pour créer des données de test
function New-TestCategory {
    $timestamp = Get-Date -Format "yyyyMMddHHmmss"
    return @{
        name = "Test Category $timestamp"
        slug = "test-category-$timestamp"
        description = "Catégorie de test créée automatiquement"
        sortOrder = 1
        isActive = $true
    }
}

function New-TestProduct {
    param([string]$CategoryId)
    
    $timestamp = Get-Date -Format "yyyyMMddHHmmss"
    return @{
        name = "Test Product $timestamp"
        sku = "TEST-$timestamp"
        description = "Produit de test créé automatiquement"
        price = 99.99
        categoryId = $CategoryId
        trackQuantity = $true
        quantity = 100
        isActive = $true
        isFeatured = $false
    }
}

# Fonction pour afficher le résumé
function Show-Summary {
    Write-Host "`n📊 RÉSUMÉ DES TESTS" -ForegroundColor Magenta
    Write-Host "===================" -ForegroundColor Magenta
    Write-Host "Total: $script:TestCount tests" -ForegroundColor White
    Write-Host "Succès: $script:SuccessCount" -ForegroundColor Green
    Write-Host "Échecs: $script:FailCount" -ForegroundColor Red
    
    $successRate = if ($script:TestCount -gt 0) { 
        [math]::Round(($script:SuccessCount / $script:TestCount) * 100, 2) 
    } else { 0 }
    
    Write-Host "Taux de succès: $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })
    
    if ($script:FailCount -gt 0) {
        Write-Host "`n❌ Tests échoués:" -ForegroundColor Red
        $script:Results | Where-Object { -not $_.Success } | ForEach-Object {
            Write-Host "   - $($_.Name): $($_.Error)" -ForegroundColor Red
        }
    }
    
    # Temps de réponse moyen
    $avgDuration = if ($script:Results.Count -gt 0) {
        [math]::Round(($script:Results | Measure-Object Duration -Average).Average, 2)
    } else { 0 }
    Write-Host "Temps de réponse moyen: ${avgDuration}ms" -ForegroundColor Cyan
}

# EXÉCUTION DES TESTS
Write-Host "🚀 Début des tests..." -ForegroundColor Blue

# Test 1: Health Check / Ping
Invoke-Test -Name "Health Check" -Endpoint "/health" -ExpectSuccess:$false

# Test 2: Swagger disponible
Invoke-Test -Name "Swagger UI" -Endpoint "/swagger" -ExpectedStatusCodes @(200, 301, 302) -ExpectSuccess:$false

# Test 3: Récupérer toutes les catégories
$categoriesResult = Invoke-Test -Name "Get All Categories" -Endpoint "/api/v1/Categories"

# Test 4: Récupérer tous les produits
$productsResult = Invoke-Test -Name "Get All Products" -Endpoint "/api/v1/Products"

# Test 5: Créer une nouvelle catégorie
$newCategory = New-TestCategory
$createCategoryResult = Invoke-Test -Name "Create Category" -Method "POST" -Endpoint "/api/v1/Categories" -Body $newCategory

$createdCategoryId = $null
if ($createCategoryResult.Success -and $createCategoryResult.Response.data) {
    $createdCategoryId = $createCategoryResult.Response.data.id
    Write-Host "   📝 Catégorie créée avec ID: $createdCategoryId" -ForegroundColor Cyan
    
    # Test 6: Récupérer la catégorie créée
    Invoke-Test -Name "Get Created Category" -Endpoint "/api/v1/Categories/$createdCategoryId"
}

# Test 7: Créer un nouveau produit (si on a une catégorie)
if ($createdCategoryId) {
    $newProduct = New-TestProduct -CategoryId $createdCategoryId
    $createProductResult = Invoke-Test -Name "Create Product" -Method "POST" -Endpoint "/api/v1/Products" -Body $newProduct
    
    $createdProductId = $null
    if ($createProductResult.Success -and $createProductResult.Response.data) {
        $createdProductId = $createProductResult.Response.data.id
        Write-Host "   📝 Produit créé avec ID: $createdProductId" -ForegroundColor Cyan
        
        # Test 8: Récupérer le produit créé
        Invoke-Test -Name "Get Created Product" -Endpoint "/api/v1/Products/$createdProductId"
        
        # Test 9: Mettre à jour le produit
        $updateProduct = @{
            id = $createdProductId
            name = "Updated Test Product"
            sku = $newProduct.sku
            description = "Produit mis à jour"
            price = 149.99
            categoryId = $createdCategoryId
            trackQuantity = $true
            quantity = 50
            isActive = $true
            isFeatured = $true
        }
        Invoke-Test -Name "Update Product" -Method "PUT" -Endpoint "/api/v1/Products/$createdProductId" -Body $updateProduct
        
        # Test 10: Supprimer le produit
        Invoke-Test -Name "Delete Product" -Method "DELETE" -Endpoint "/api/v1/Products/$createdProductId"
    }
}

# Test 11: Tests de filtrage
Invoke-Test -Name "Filter Products by Category" -Endpoint "/api/v1/Products?categoryId=$createdCategoryId" -ExpectSuccess:$true
Invoke-Test -Name "Search Products" -Endpoint "/api/v1/Products?searchTerm=test" -ExpectSuccess:$true
Invoke-Test -Name "Products with Pagination" -Endpoint "/api/v1/Products?pageNumber=1&pageSize=5" -ExpectSuccess:$true

# Test 12: Tests d'erreurs
Invoke-Test -Name "Get Non-existent Category" -Endpoint "/api/v1/Categories/00000000-0000-0000-0000-000000000000" -ExpectedStatusCodes @(404, 400) -ExpectSuccess:$false
Invoke-Test -Name "Get Non-existent Product" -Endpoint "/api/v1/Products/00000000-0000-0000-0000-000000000000" -ExpectedStatusCodes @(404, 400) -ExpectSuccess:$false

# Affichage du résumé final
Show-Summary

# Code de sortie basé sur les résultats
if ($script:FailCount -eq 0) {
    Write-Host "`n🎉 Tous les tests sont passés!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n⚠️  Certains tests ont échoué. Vérifiez les détails ci-dessus." -ForegroundColor Yellow
    exit 1
}