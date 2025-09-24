# Tests Auth.API - Suite finale
Write-Host "TESTS COMPLETS AUTH.API" -ForegroundColor Green

$baseUrl = "http://localhost:5001"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

$testUser = @{
    username = "finaltest_$timestamp"
    email = "finaltest_$timestamp@niespro.com" 
    password = "FinalTest123!"
    firstName = "Final"
    lastName = "Test"
    deviceKey = "final-device-$timestamp"
}

Write-Host "Utilisateur de test: $($testUser.username)" -ForegroundColor Cyan

# Test 1: Health Check
Write-Host "TEST 1: Health Check" -ForegroundColor Yellow
try {
    $health = Invoke-WebRequest -Uri "$baseUrl/health" -UseBasicParsing
    Write-Host "PASS Health Check (Status: $($health.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "FAIL Health Check: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Enregistrement
Write-Host "TEST 2: Enregistrement utilisateur" -ForegroundColor Yellow
$registerData = @{
    username = $testUser.username
    email = $testUser.email
    password = $testUser.password
    confirmPassword = $testUser.password
    firstName = $testUser.firstName
    lastName = $testUser.lastName
    phoneNumber = "+33123456789"
    deviceKey = $testUser.deviceKey
    deviceName = "Final Test Device"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    if ($registerResponse.success) {
        Write-Host "PASS Enregistrement - UserId: $($registerResponse.data.userId)" -ForegroundColor Green
    } else {
        Write-Host "FAIL Enregistrement: $($registerResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR Enregistrement: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Validation unicite username
Write-Host "TEST 3: Validation unicite username" -ForegroundColor Yellow
$duplicateUsernameData = @{
    username = $testUser.username
    email = "different_$($testUser.email)"
    password = $testUser.password
    confirmPassword = $testUser.password
    firstName = "Duplicate"
    lastName = "Username"
    phoneNumber = "+33987654321"
    deviceKey = "duplicate-username-device"
    deviceName = "Duplicate Username Test"
} | ConvertTo-Json

try {
    $duplicateResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/register" -Method POST -Body $duplicateUsernameData -ContentType "application/json"
    Write-Host "FAIL Username validation - Should have been rejected" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "PASS Username validation - Duplicate rejected" -ForegroundColor Green
    } else {
        Write-Host "ERROR Unexpected: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Test 4: Connexion
Write-Host "TEST 4: Connexion utilisateur" -ForegroundColor Yellow
$loginData = @{
    email = $testUser.email
    password = $testUser.password
    deviceKey = $testUser.deviceKey
    deviceName = "Final Test Device Login"
    rememberMe = $true
} | ConvertTo-Json

$accessToken = $null
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    if ($loginResponse.success -and $loginResponse.data.accessToken) {
        Write-Host "PASS Connexion - Token generated" -ForegroundColor Green
        $accessToken = $loginResponse.data.accessToken
        Write-Host "Token: $($accessToken.Substring(0,30))..." -ForegroundColor Blue
    } else {
        Write-Host "FAIL Connexion: $($loginResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR Connexion: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Mauvaises credentials
Write-Host "TEST 5: Mauvaises credentials" -ForegroundColor Yellow
$badLoginData = @{
    email = $testUser.email
    password = "WrongPassword123!"
    deviceKey = "bad-device-key"
    deviceName = "Bad Login Test"
} | ConvertTo-Json

try {
    $badResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/login" -Method POST -Body $badLoginData -ContentType "application/json"
    Write-Host "FAIL Bad credentials accepted" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "PASS Bad credentials rejected" -ForegroundColor Green
    } else {
        Write-Host "ERROR Unexpected: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "TESTS TERMINES - AUTH.API FINALISE!" -ForegroundColor Green