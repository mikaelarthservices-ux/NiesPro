# ===============================================
# Script de Test Auth.API - NiesPro
# ===============================================

param(
    [switch]$TestRegistration,
    [switch]$TestLogin,
    [switch]$TestDatabase,
    [switch]$All
)

function Write-Header {
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "TEST AUTH.API - NIESPRO" -ForegroundColor Cyan
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Test-AuthHealth {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5001/health" -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "Auth.API Health: OK" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "Auth.API Health: ERREUR" -ForegroundColor Red
        return $false
    }
}

function Test-AuthSwagger {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5001/swagger" -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "Auth.API Swagger: OK" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "Auth.API Swagger: ERREUR" -ForegroundColor Red
        return $false
    }
}

function Test-AuthRegistration {
    Write-Host "=== TEST REGISTRATION ===" -ForegroundColor White
    
    $testUser = @{
        Username = "testuser$(Get-Random)"
        Email = "test$(Get-Random)@niespro.com"
        Password = "TestPass123!"
        ConfirmPassword = "TestPass123!"
        FirstName = "Test"
        LastName = "User"
        DeviceKey = "test-device-$(Get-Random)"
        DeviceName = "Test Device"
    } | ConvertTo-Json
    
    try {
        Write-Host "Tentative de registration..." -ForegroundColor Yellow
        $response = Invoke-WebRequest -Uri "http://localhost:5001/api/v1/auth/register" -Method Post -Body $testUser -ContentType "application/json" -UseBasicParsing
        
        if ($response.StatusCode -eq 201) {
            Write-Host "Registration: SUCCES" -ForegroundColor Green
            $result = $response.Content | ConvertFrom-Json
            Write-Host "Utilisateur créé: $($result.data.email)" -ForegroundColor Gray
            return $true
        } else {
            Write-Host "Registration: Status $($response.StatusCode)" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "Registration: ERREUR" -ForegroundColor Red
        if ($_.Exception.Response) {
            $errorResponse = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorResponse)
            $errorContent = $reader.ReadToEnd()
            Write-Host "Détails: $errorContent" -ForegroundColor Red
        }
        return $false
    }
}

function Test-AuthLogin {
    Write-Host "=== TEST LOGIN ===" -ForegroundColor White
    
    # Tentative avec des identifiants par défaut
    $loginData = @{
        Email = "admin@niespro.com"
        Password = "Admin123!"
        DeviceKey = "test-device-login"
    } | ConvertTo-Json
    
    try {
        Write-Host "Tentative de login..." -ForegroundColor Yellow
        $response = Invoke-WebRequest -Uri "http://localhost:5001/api/v1/auth/login" -Method Post -Body $loginData -ContentType "application/json" -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Host "Login: SUCCES" -ForegroundColor Green
            $result = $response.Content | ConvertFrom-Json
            if ($result.data.accessToken) {
                Write-Host "Token reçu: $($result.data.accessToken.Substring(0, 50))..." -ForegroundColor Gray
            }
            return $true
        } else {
            Write-Host "Login: Status $($response.StatusCode)" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "Login: ERREUR" -ForegroundColor Red
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode
            Write-Host "Code: $statusCode" -ForegroundColor Red
        }
        return $false
    }
}

function Test-AuthDatabase {
    Write-Host "=== TEST BASE DE DONNEES ===" -ForegroundColor White
    
    try {
        # Test EF Core migration
        Write-Host "Test des migrations EF Core..." -ForegroundColor Yellow
        Push-Location "C:\Users\HP\Documents\projets\NiesPro\src\Services\Auth\Auth.API"
        
        $migrationResult = & dotnet ef database list 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Migrations EF: OK" -ForegroundColor Green
        } else {
            Write-Host "Migrations EF: ERREUR" -ForegroundColor Red
        }
        
        Pop-Location
    }
    catch {
        Write-Host "Test EF Core: ERREUR" -ForegroundColor Red
    }
}

# Début des tests
Write-Header

# Test de base
Write-Host "=== TESTS DE BASE ===" -ForegroundColor White
$healthOk = Test-AuthHealth
$swaggerOk = Test-AuthSwagger

if (-not $healthOk) {
    Write-Host ""
    Write-Host "ATTENTION: Auth.API ne répond pas sur le port 5001" -ForegroundColor Red
    Write-Host "Vérifiez que le service est démarré" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Tests spécifiques
if ($TestDatabase -or $All) {
    Test-AuthDatabase
    Write-Host ""
}

if ($TestRegistration -or $All) {
    Test-AuthRegistration
    Write-Host ""
}

if ($TestLogin -or $All) {
    Test-AuthLogin
    Write-Host ""
}

if (-not ($TestRegistration -or $TestLogin -or $TestDatabase -or $All)) {
    Write-Host "Usage: .\test-auth.ps1 [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor White
    Write-Host "  -TestRegistration : Tester la création d'utilisateur"
    Write-Host "  -TestLogin        : Tester la connexion"
    Write-Host "  -TestDatabase     : Tester la base de données"
    Write-Host "  -All              : Tous les tests"
}

Write-Host "Tests terminés: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray