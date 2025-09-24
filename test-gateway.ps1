# Script de test Gateway.API corrigé
Write-Host "🧪 TEST GATEWAY.API - CONFIGURATION CORRIGÉE" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Yellow

# Test 1: Health check Gateway
Write-Host "`n🔍 Test 1: Health check Gateway..." -ForegroundColor Cyan
try {
    $gateway = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing
    Write-Host "✅ Gateway Health: $($gateway.Content)" -ForegroundColor Green
} catch {
    Write-Host "❌ Gateway Health: FAILED" -ForegroundColor Red
}

# Test 2: Routage vers Auth.API  
Write-Host "`n🔍 Test 2: Routage Auth.API..." -ForegroundColor Cyan
try {
    $auth = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/health" -UseBasicParsing
    Write-Host "✅ Auth via Gateway: $($auth.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Auth via Gateway: FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Routage vers Catalog.API
Write-Host "`n🔍 Test 3: Routage Catalog.API..." -ForegroundColor Cyan  
try {
    $catalog = Invoke-WebRequest -Uri "http://localhost:5000/api/products/health" -UseBasicParsing
    Write-Host "✅ Catalog via Gateway: $($catalog.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Catalog via Gateway: FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Routage vers Stock.API
Write-Host "`n🔍 Test 4: Routage Stock.API..." -ForegroundColor Cyan
try {
    $stock = Invoke-WebRequest -Uri "http://localhost:5000/api/stock/health" -UseBasicParsing  
    Write-Host "✅ Stock via Gateway: $($stock.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Stock via Gateway: FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Swagger Gateway
Write-Host "`n🔍 Test 5: Swagger Gateway..." -ForegroundColor Cyan
try {
    $swagger = Invoke-WebRequest -Uri "http://localhost:5000/swagger" -UseBasicParsing
    Write-Host "✅ Swagger Gateway: Accessible" -ForegroundColor Green
} catch {
    Write-Host "❌ Swagger Gateway: FAILED" -ForegroundColor Red
}

Write-Host "`n🎯 RÉSUMÉ DES CORRECTIONS APPLIQUÉES:" -ForegroundColor Yellow
Write-Host "- ✅ Configuration Kestrel ajoutée (ports 5000/5010)" -ForegroundColor Green
Write-Host "- ✅ Tous les services ajoutés dans configuration" -ForegroundColor Green  
Write-Host "- ✅ Table de routage complète (Auth, Catalog, Order, Payment, Stock)" -ForegroundColor Green
Write-Host "- ✅ URLs HTTPS corrigées pour tous les services" -ForegroundColor Green