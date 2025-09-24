# Tests complets Auth.API - Suite de validation finale
Write-Host "=== TESTS COMPLETS AUTH.API ===" -ForegroundColor Green

$baseUrl = "http://localhost:5001"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# Donnees de test uniques
$testUser = @{
    username = "finaltest_$timestamp"
    email = "finaltest_$timestamp@niespro.com"
    password = "FinalTest123!"
    firstName = "Final"
    lastName = "Test"
    deviceKey = "final-device-$timestamp"
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [hashtable]$Body = @{},
        [hashtable]$Headers = @{},
        [int]$ExpectedStatus = 200
    )
    
    Write-Host "`n🧪 Test: $Name" -ForegroundColor Cyan
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            UseBasicParsing = $true
            TimeoutSec = 10
        }
        
        if ($Body.Count -gt 0) {
            $params.Body = ($Body | ConvertTo-Json -Depth 3)
            $params.ContentType = "application/json"
        }
        
        if ($Headers.Count -gt 0) {
            $params.Headers = $Headers
        }
        
        $response = Invoke-WebRequest @params
        $content = $response.Content | ConvertFrom-Json
        
        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-Host "✅ PASS - Status: $($response.StatusCode)" -ForegroundColor Green
            $script:testResults += [PSCustomObject]@{
                Test = $Name
                Status = "PASS"
                StatusCode = $response.StatusCode
                Message = $content.message
            }
            return $content
        } else {
            Write-Host "❌ FAIL - Status: $($response.StatusCode), Expected: $ExpectedStatus" -ForegroundColor Red
            $script:testResults += [PSCustomObject]@{
                Test = $Name
                Status = "FAIL"
                StatusCode = $response.StatusCode
                Message = "Status mismatch"
            }
            return $null
        }
    } catch {
        Write-Host "❌ ERROR - $($_.Exception.Message)" -ForegroundColor Red
        $script:testResults += [PSCustomObject]@{
            Test = $Name
            Status = "ERROR"
            StatusCode = "N/A"
            Message = $_.Exception.Message
        }
        return $null
    }
}

# Test 1: Health Check
Write-Host "`n🔍 TEST 1: Health Check" -ForegroundColor Yellow
Test-Endpoint -Name "Health Check" -Method "GET" -Url "$baseUrl/health"

# Test 2: Enregistrement avec validations d'unicité
Write-Host "`n🔍 TEST 2: Enregistrement utilisateur (avec validations)" -ForegroundColor Yellow
$registerResult = Test-Endpoint -Name "User Registration" -Method "POST" -Url "$baseUrl/api/v1/auth/register" -Body @{
    username = $testUser.username
    email = $testUser.email
    password = $testUser.password
    confirmPassword = $testUser.password
    firstName = $testUser.firstName
    lastName = $testUser.lastName
    phoneNumber = "+33123456789"
    deviceKey = "test-device-final-" + (Get-Date -Format "yyyyMMddHHmmss")
    deviceName = "Final Test Device"
} -ExpectedStatus 200

if ($registerResult) {
    Write-Host "   📝 Utilisateur créé: $($registerResult.data.userId)" -ForegroundColor Blue
}

# Test 3: Test de validation d'unicité - Même username
Write-Host "`n🔍 TEST 3: Validation unicité username" -ForegroundColor Yellow
Test-Endpoint -Name "Duplicate Username Validation" -Method "POST" -Url "$baseUrl/api/v1/auth/register" -Body @{
    username = $testUser.username  # Même username
    email = "different_$($testUser.email)"
    password = $testUser.password
    confirmPassword = $testUser.password
    firstName = $testUser.firstName
    lastName = $testUser.lastName
    phoneNumber = "+33123456789"
    deviceKey = "test-device-duplicate-username"
    deviceName = "Duplicate Username Test"
} -ExpectedStatus 400

# Test 4: Test de validation d'unicité - Même email
Write-Host "`n🔍 TEST 4: Validation unicité email" -ForegroundColor Yellow
Test-Endpoint -Name "Duplicate Email Validation" -Method "POST" -Url "$baseUrl/api/v1/auth/register" -Body @{
    username = "different_$($testUser.username)"
    email = $testUser.email  # Même email
    password = $testUser.password
    confirmPassword = $testUser.password
    firstName = $testUser.firstName
    lastName = $testUser.lastName
    phoneNumber = "+33123456789"
    deviceKey = "test-device-duplicate-email"
    deviceName = "Duplicate Email Test"
} -ExpectedStatus 400

# Test 5: Connexion utilisateur
Write-Host "`n🔍 TEST 5: Connexion utilisateur" -ForegroundColor Yellow
$loginResult = Test-Endpoint -Name "User Login" -Method "POST" -Url "$baseUrl/api/v1/auth/login" -Body @{
    email = $testUser.email
    password = $testUser.password
    deviceKey = "test-device-final-" + (Get-Date -Format "yyyyMMddHHmmss")
    deviceName = "Final Test Device"
    rememberMe = $true
}

$accessToken = $null
if ($loginResult -and $loginResult.data.accessToken) {
    $accessToken = $loginResult.data.accessToken
    Write-Host "   🔑 Token généré: $($accessToken.Substring(0,30))..." -ForegroundColor Blue
}

# Test 6: Endpoint protégé (avec token)
Write-Host "`n🔍 TEST 6: Endpoint protégé (Logout)" -ForegroundColor Yellow
if ($accessToken) {
    Test-Endpoint -Name "Protected Endpoint (Logout)" -Method "POST" -Url "$baseUrl/api/v1/auth/logout" -Headers @{
        "Authorization" = "Bearer $accessToken"
    }
} else {
    Write-Host "❌ Pas de token disponible pour tester l'endpoint protégé" -ForegroundColor Red
}

# Test 7: Connexion avec mauvaises credentials
Write-Host "`n🔍 TEST 7: Connexion avec mauvaises credentials" -ForegroundColor Yellow
Test-Endpoint -Name "Invalid Credentials" -Method "POST" -Url "$baseUrl/api/v1/auth/login" -Body @{
    email = $testUser.email
    password = "WrongPassword123!"
    deviceKey = "test-device-invalid"
    deviceName = "Invalid Test"
} -ExpectedStatus 401

# Résumé des tests
Write-Host "`n📊 RÉSUMÉ DES TESTS" -ForegroundColor Green
Write-Host "===================" -ForegroundColor Green

$passedTests = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failedTests = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$errorTests = ($testResults | Where-Object { $_.Status -eq "ERROR" }).Count
$totalTests = $testResults.Count

Write-Host "Total tests: $totalTests" -ForegroundColor White
Write-Host "✅ Passed: $passedTests" -ForegroundColor Green
Write-Host "❌ Failed: $failedTests" -ForegroundColor Red
Write-Host "🚨 Errors: $errorTests" -ForegroundColor Yellow

Write-Host "`nDétail des résultats:" -ForegroundColor Cyan
$testResults | Format-Table -AutoSize

if ($failedTests -eq 0 -and $errorTests -eq 0) {
    Write-Host "`n🎉 TOUS LES TESTS RÉUSSIS - AUTH.API FINALISÉ!" -ForegroundColor Green
    Write-Host "Auth.API est prêt pour la production!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Des tests ont échoué - Vérification nécessaire" -ForegroundColor Yellow
}

Write-Host "`n=== FIN DES TESTS ===" -ForegroundColor Green