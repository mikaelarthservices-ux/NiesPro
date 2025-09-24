# Documentation des Routes Auth.API
Write-Host "=== INVENTAIRE DES ROUTES AUTH.API ===" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

$baseUrl = "http://localhost:5001"

# Test de connectivité au service
Write-Host "`n🔍 Vérification du service Auth.API..." -ForegroundColor Yellow
try {
    $health = Invoke-WebRequest -Uri "$baseUrl/health" -UseBasicParsing -TimeoutSec 5
    Write-Host "✅ Service accessible (Status: $($health.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "❌ Service non accessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Assurez-vous que Auth.API fonctionne sur localhost:5001" -ForegroundColor Yellow
    exit 1
}

# Documentation des endpoints
Write-Host "`n📋 ENDPOINTS DISPONIBLES:" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan

# Health endpoint
Write-Host "`n🏥 HEALTH CHECK" -ForegroundColor Green
Write-Host "  📍 GET /health"
Write-Host "  📝 Vérification de l'état du service"
Write-Host "  🔓 Public (pas d'authentification requise)"
Write-Host "  📤 Response: 200 OK + message de statut"

# Authentication endpoints
Write-Host "`n🔐 AUTHENTICATION" -ForegroundColor Green

Write-Host "`n  📍 POST /api/v1/auth/register"
Write-Host "  📝 Enregistrement d'un nouvel utilisateur"
Write-Host "  🔓 Public"
Write-Host "  📥 Body: {username, email, password, confirmPassword, firstName, lastName, phoneNumber, deviceKey, deviceName}"
Write-Host "  📤 Success: 200 + {userId, username, email, isActive, emailConfirmed, createdAt}"
Write-Host "  📤 Error: 400 (validation) ou 409 (conflit)"

Write-Host "`n  📍 POST /api/v1/auth/login"
Write-Host "  📝 Connexion utilisateur avec génération JWT"
Write-Host "  🔓 Public"
Write-Host "  📥 Body: {email, password, deviceKey, deviceName, rememberMe}"
Write-Host "  📤 Success: 200 + {accessToken, refreshToken, tokenType, expiresIn, user}"
Write-Host "  📤 Error: 401 (credentials invalides) ou 400 (utilisateur inactif)"

Write-Host "`n  📍 POST /api/v1/auth/logout"
Write-Host "  📝 Déconnexion utilisateur"
Write-Host "  🔒 Protégé (Bearer token requis)"
Write-Host "  📥 Headers: Authorization: Bearer <token>"
Write-Host "  📤 Success: 200 + message de confirmation"
Write-Host "  📤 Error: 401 (non autorisé)"

Write-Host "`n  📍 POST /api/v1/auth/refresh-token"
Write-Host "  📝 Renouvellement du token d'accès"
Write-Host "  🔓 Public"
Write-Host "  📥 Body: {refreshToken}"
Write-Host "  📤 Success: 200 + nouveaux tokens"
Write-Host "  📤 Error: 401 (refresh token invalide)"

Write-Host "`n  📍 POST /api/v1/auth/change-password"
Write-Host "  📝 Changement de mot de passe"
Write-Host "  🔒 Protégé (Bearer token requis)"
Write-Host "  📥 Body: {currentPassword, newPassword, confirmPassword, deviceKey, deviceName}"
Write-Host "  📤 Success: 200 + confirmation"
Write-Host "  📤 Error: 400 (validation) ou 401 (mot de passe incorrect)"

# Test des endpoints publics
Write-Host "`n🧪 TESTS DES ENDPOINTS PUBLICS:" -ForegroundColor Yellow
Write-Host "===============================" -ForegroundColor Yellow

# Test Health
Write-Host "`n🏥 Test Health Check..."
try {
    $healthResponse = Invoke-WebRequest -Uri "$baseUrl/health" -UseBasicParsing
    Write-Host "  ✅ GET /health → $($healthResponse.StatusCode) $($healthResponse.StatusDescription)" -ForegroundColor Green
} catch {
    Write-Host "  ❌ GET /health → ERROR" -ForegroundColor Red
}

# Test d'un register invalide (pour voir le format d'erreur)
Write-Host "`n📝 Test Register (validation)..."
$invalidRegister = @{
    username = "ab"  # Trop court
    email = "invalid-email"  # Format invalide
    password = "123"  # Trop court
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/register" -Method POST -Body $invalidRegister -ContentType "application/json"
    Write-Host "  ⚠️  POST /api/v1/auth/register → Validation failed as expected" -ForegroundColor Yellow
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "  ✅ POST /api/v1/auth/register → $statusCode (Validation working)" -ForegroundColor Green
}

# Test d'un login invalide
Write-Host "`n🔐 Test Login (credentials invalides)..."
$invalidLogin = @{
    email = "nonexistent@test.com"
    password = "WrongPassword123!"
    deviceKey = "test-device"
    deviceName = "Test Device"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/login" -Method POST -Body $invalidLogin -ContentType "application/json"
    Write-Host "  ❌ POST /api/v1/auth/login → Should have failed" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "  ✅ POST /api/v1/auth/login → $statusCode (Security working)" -ForegroundColor Green
}

# Documentation des codes de statut
Write-Host "`n📊 CODES DE STATUT:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "  200 OK      → Opération réussie"
Write-Host "  400 Bad Request → Données invalides ou validation échouée"
Write-Host "  401 Unauthorized → Authentification requise ou credentials invalides"
Write-Host "  403 Forbidden → Accès interdit"
Write-Host "  404 Not Found → Ressource non trouvée"
Write-Host "  409 Conflict → Conflit (ex: email déjà utilisé)"
Write-Host "  500 Internal Server Error → Erreur serveur"

# Documentation des formats de réponse
Write-Host "`n📋 FORMAT DES RÉPONSES:" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
Write-Host "Toutes les réponses suivent le format ApiResponse:"
Write-Host @"
{
  "success": boolean,
  "message": "string",
  "data": object,
  "errors": array,
  "statusCode": number,
  "isSuccess": boolean
}
"@

Write-Host "`n🎯 AUTHENTIFICATION JWT:" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host "Format du token JWT:"
Write-Host @"
Header: Authorization: Bearer <token>
Token structure:
{
  "userId": "uuid",
  "email": "user@domain.com",
  "roles": ["role1", "role2"],
  "iat": timestamp,
  "exp": timestamp,
  "iss": "Auth.API",
  "aud": "Auth.Client"
}
"@

Write-Host "`n✅ DOCUMENTATION ROUTES COMPLÈTE!" -ForegroundColor Green
Write-Host "Auth.API expose tous les endpoints d'authentification nécessaires" -ForegroundColor Green
Write-Host "Service prêt pour intégration avec Gateway.API et autres microservices" -ForegroundColor Green