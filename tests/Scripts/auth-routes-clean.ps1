# =================================================================
# DOCUMENTATION COMPLETE DES ROUTES AUTH.API
# =================================================================

Write-Host "=====================================================" -ForegroundColor Yellow
Write-Host "    DOCUMENTATION ROUTES AUTH.API - Service d'Authentification" -ForegroundColor Yellow
Write-Host "=====================================================" -ForegroundColor Yellow
Write-Host ""

# Configuration
$authBaseUrl = "https://localhost:5011"

Write-Host "INFORMATIONS DU SERVICE:" -ForegroundColor Cyan
Write-Host "  - URL Base: $authBaseUrl" -ForegroundColor White
Write-Host "  - Version: .NET 8" -ForegroundColor White
Write-Host "  - Base de donnees: MySQL" -ForegroundColor White
Write-Host "  - Authentification: JWT Bearer Token" -ForegroundColor White
Write-Host ""

Write-Host "ROUTES DISPONIBLES:" -ForegroundColor Green
Write-Host ""

# =================================================================
# 1. ROUTE HEALTH CHECK
# =================================================================
Write-Host "1. HEALTH CHECK" -ForegroundColor Magenta
Write-Host "  Methode: GET" -ForegroundColor White
Write-Host "  Endpoint: $authBaseUrl/health" -ForegroundColor Yellow
Write-Host "  Description: Verification de l'etat du service" -ForegroundColor Gray
Write-Host "  Authentification: Aucune" -ForegroundColor Gray
Write-Host "  Reponse Success: 200 - 'Healthy'" -ForegroundColor Green
Write-Host ""

# =================================================================
# 2. ROUTE INSCRIPTION (REGISTER)
# =================================================================
Write-Host "2. INSCRIPTION UTILISATEUR" -ForegroundColor Magenta
Write-Host "  Methode: POST" -ForegroundColor White
Write-Host "  Endpoint: $authBaseUrl/api/v1/auth/register" -ForegroundColor Yellow
Write-Host "  Description: Creation d'un nouveau compte utilisateur" -ForegroundColor Gray
Write-Host "  Authentification: Aucune" -ForegroundColor Gray
Write-Host "  Body JSON:" -ForegroundColor Cyan
Write-Host "  {" -ForegroundColor White
Write-Host "    'username': 'string (requis, unique)'," -ForegroundColor White
Write-Host "    'email': 'string (requis, unique, format email)'," -ForegroundColor White
Write-Host "    'password': 'string (requis, min 8 caracteres)'," -ForegroundColor White
Write-Host "    'confirmPassword': 'string (requis, doit correspondre)'" -ForegroundColor White
Write-Host "  }" -ForegroundColor White
Write-Host "  Reponse Success: 201 + tokens JWT" -ForegroundColor Green
Write-Host "  Erreurs: 400 (validation), 409 (utilisateur existe)" -ForegroundColor Red
Write-Host ""

# =================================================================
# 3. ROUTE CONNEXION (LOGIN)
# =================================================================
Write-Host "3. CONNEXION UTILISATEUR" -ForegroundColor Magenta
Write-Host "  Methode: POST" -ForegroundColor White
Write-Host "  Endpoint: $authBaseUrl/api/v1/auth/login" -ForegroundColor Yellow
Write-Host "  Description: Authentification d'un utilisateur existant" -ForegroundColor Gray
Write-Host "  Authentification: Aucune" -ForegroundColor Gray
Write-Host "  Body JSON:" -ForegroundColor Cyan
Write-Host "  {" -ForegroundColor White
Write-Host "    'username': 'string (requis)'," -ForegroundColor White
Write-Host "    'password': 'string (requis)'" -ForegroundColor White
Write-Host "  }" -ForegroundColor White
Write-Host "  Reponse Success: 200 + tokens JWT" -ForegroundColor Green
Write-Host "  {" -ForegroundColor White
Write-Host "    'accessToken': 'JWT token'," -ForegroundColor White
Write-Host "    'refreshToken': 'Refresh token'," -ForegroundColor White
Write-Host "    'tokenType': 'Bearer'," -ForegroundColor White
Write-Host "    'expiresIn': 3600" -ForegroundColor White
Write-Host "  }" -ForegroundColor White
Write-Host "  Erreurs: 401 (credentials invalides)" -ForegroundColor Red
Write-Host ""

# =================================================================
# 4. ROUTE DECONNEXION (LOGOUT)
# =================================================================
Write-Host "4. DECONNEXION UTILISATEUR" -ForegroundColor Magenta
Write-Host "  Methode: POST" -ForegroundColor White
Write-Host "  Endpoint: $authBaseUrl/api/v1/auth/logout" -ForegroundColor Yellow
Write-Host "  Description: Deconnexion et invalidation du token" -ForegroundColor Gray
Write-Host "  Authentification: Bearer Token requis" -ForegroundColor Red
Write-Host "  Header: Authorization: Bearer <votre-token>" -ForegroundColor Cyan
Write-Host "  Body: Aucun" -ForegroundColor Gray
Write-Host "  Reponse Success: 200 + message confirmation" -ForegroundColor Green
Write-Host "  Erreurs: 401 (token invalide/manquant)" -ForegroundColor Red
Write-Host ""

# Test de connectivite
Write-Host "TEST DE CONNECTIVITE:" -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$authBaseUrl/health" -Method GET -SkipCertificateCheck
    Write-Host "  Status: Service ACTIF" -ForegroundColor Green
    Write-Host "  Reponse: $healthResponse" -ForegroundColor Green
} catch {
    Write-Host "  Status: Service INACTIF ou INACCESSIBLE" -ForegroundColor Red
    Write-Host "  Erreur: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test Swagger/OpenAPI
Write-Host "DOCUMENTATION SWAGGER:" -ForegroundColor Cyan
Write-Host "  URL: $authBaseUrl/swagger" -ForegroundColor Yellow
Write-Host "  Description: Documentation interactive des API" -ForegroundColor Gray
Write-Host ""

Write-Host "EXEMPLES D'UTILISATION:" -ForegroundColor Green
Write-Host ""

Write-Host "# 1. Test Health Check" -ForegroundColor White
Write-Host "curl -k '$authBaseUrl/health'" -ForegroundColor Gray
Write-Host ""

Write-Host "# 2. Inscription nouvel utilisateur" -ForegroundColor White
Write-Host "curl -k -X POST '$authBaseUrl/api/v1/auth/register' \" -ForegroundColor Gray
Write-Host "  -H 'Content-Type: application/json' \" -ForegroundColor Gray
Write-Host "  -d '{" -ForegroundColor Gray
Write-Host "    \"username\": \"testuser\"," -ForegroundColor Gray
Write-Host "    \"email\": \"test@example.com\"," -ForegroundColor Gray
Write-Host "    \"password\": \"SecurePass123\"," -ForegroundColor Gray
Write-Host "    \"confirmPassword\": \"SecurePass123\"" -ForegroundColor Gray
Write-Host "  }'" -ForegroundColor Gray
Write-Host ""

Write-Host "# 3. Connexion utilisateur" -ForegroundColor White
Write-Host "curl -k -X POST '$authBaseUrl/api/v1/auth/login' \" -ForegroundColor Gray
Write-Host "  -H 'Content-Type: application/json' \" -ForegroundColor Gray
Write-Host "  -d '{" -ForegroundColor Gray
Write-Host "    \"username\": \"testuser\"," -ForegroundColor Gray
Write-Host "    \"password\": \"SecurePass123\"" -ForegroundColor Gray
Write-Host "  }'" -ForegroundColor Gray
Write-Host ""

Write-Host "# 4. Deconnexion (avec token)" -ForegroundColor White
Write-Host "curl -k -X POST '$authBaseUrl/api/v1/auth/logout' \" -ForegroundColor Gray
Write-Host "  -H 'Authorization: Bearer YOUR_TOKEN_HERE'" -ForegroundColor Gray
Write-Host ""

Write-Host "SECURITE ET BONNES PRATIQUES:" -ForegroundColor Red
Write-Host "  - Toujours utiliser HTTPS en production" -ForegroundColor Yellow
Write-Host "  - Stocker les tokens de maniere securisee" -ForegroundColor Yellow
Write-Host "  - Implementer la rotation des refresh tokens" -ForegroundColor Yellow
Write-Host "  - Valider et assainir toutes les entrees" -ForegroundColor Yellow
Write-Host "  - Configurer CORS appropriement" -ForegroundColor Yellow
Write-Host ""

Write-Host "=====================================================" -ForegroundColor Green
Write-Host "    DOCUMENTATION ROUTES AUTH.API COMPLETE!" -ForegroundColor Green
Write-Host "    Toutes les routes d'authentification documentees" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green