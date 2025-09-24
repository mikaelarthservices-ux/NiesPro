# =================================================================
# DOCUMENTATION SWAGGER AUTH.API - Endpoints Interactifs
# =================================================================

Write-Host "=====================================================" -ForegroundColor Yellow
Write-Host "    SWAGGER AUTH.API - Documentation Interactive" -ForegroundColor Yellow
Write-Host "=====================================================" -ForegroundColor Yellow
Write-Host ""

# Configuration
$swaggerHttpUrl = "http://localhost:5001/swagger"
$swaggerHttpsUrl = "https://localhost:5011/swagger"
$swaggerJsonUrl = "http://localhost:5001/swagger/v1/swagger.json"

Write-Host "ACCES A LA DOCUMENTATION SWAGGER:" -ForegroundColor Cyan
Write-Host "  - HTTP  : $swaggerHttpUrl" -ForegroundColor Green
Write-Host "  - HTTPS : $swaggerHttpsUrl" -ForegroundColor Green
Write-Host "  - JSON  : $swaggerJsonUrl" -ForegroundColor Yellow
Write-Host ""

# Test de connectivite Swagger
Write-Host "VERIFICATION DE L'ACCES SWAGGER:" -ForegroundColor Yellow
try {
    $httpResponse = Invoke-WebRequest -Uri $swaggerHttpUrl -UseBasicParsing
    Write-Host "  HTTP Swagger : ACCESSIBLE (Status: $($httpResponse.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "  HTTP Swagger : INACCESSIBLE" -ForegroundColor Red
    Write-Host "    Erreur: $($_.Exception.Message)" -ForegroundColor Red
}

try {
    $httpsResponse = Invoke-WebRequest -Uri $swaggerHttpsUrl -SkipCertificateCheck -UseBasicParsing
    Write-Host "  HTTPS Swagger: ACCESSIBLE (Status: $($httpsResponse.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "  HTTPS Swagger: INACCESSIBLE" -ForegroundColor Red
    Write-Host "    Erreur: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Recuperation du schema OpenAPI
Write-Host "RECUPERATION DU SCHEMA OPENAPI:" -ForegroundColor Cyan
try {
    $swaggerJson = Invoke-RestMethod -Uri $swaggerJsonUrl
    Write-Host "  Schema JSON : DISPONIBLE" -ForegroundColor Green
    Write-Host "  Version API : $($swaggerJson.info.version)" -ForegroundColor White
    Write-Host "  Titre       : $($swaggerJson.info.title)" -ForegroundColor White
    Write-Host "  Description : $($swaggerJson.info.description)" -ForegroundColor White
    
    Write-Host ""
    Write-Host "ENDPOINTS DETECTES DANS SWAGGER:" -ForegroundColor Magenta
    
    $endpointCount = 0
    foreach ($path in $swaggerJson.paths.PSObject.Properties) {
        $endpointPath = $path.Name
        foreach ($method in $path.Value.PSObject.Properties) {
            $httpMethod = $method.Name.ToUpper()
            $operation = $method.Value
            $endpointCount++
            
            Write-Host "  $endpointCount. $httpMethod $endpointPath" -ForegroundColor Yellow
            if ($operation.summary) {
                Write-Host "     Summary: $($operation.summary)" -ForegroundColor Gray
            }
            if ($operation.description) {
                Write-Host "     Description: $($operation.description)" -ForegroundColor Gray
            }
            if ($operation.tags) {
                Write-Host "     Tags: $($operation.tags -join ', ')" -ForegroundColor Cyan
            }
            Write-Host ""
        }
    }
    
    Write-Host "TOTAL ENDPOINTS: $endpointCount" -ForegroundColor Green
} catch {
    Write-Host "  Schema JSON : INACCESSIBLE" -ForegroundColor Red
    Write-Host "    Erreur: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "FONCTIONNALITES SWAGGER DISPONIBLES:" -ForegroundColor Green
Write-Host "  - Documentation interactive des endpoints" -ForegroundColor White
Write-Host "  - Test direct des API depuis l'interface" -ForegroundColor White
Write-Host "  - Visualisation des schemas de donnees" -ForegroundColor White
Write-Host "  - Authentification JWT integree" -ForegroundColor White
Write-Host "  - Generation de code client" -ForegroundColor White
Write-Host "  - Export du schema OpenAPI" -ForegroundColor White
Write-Host ""

Write-Host "COMMENT UTILISER SWAGGER:" -ForegroundColor Cyan
Write-Host "  1. Ouvrir $swaggerHttpUrl dans votre navigateur" -ForegroundColor White
Write-Host "  2. Explorer les endpoints disponibles" -ForegroundColor White
Write-Host "  3. Cliquer sur 'Try it out' pour tester un endpoint" -ForegroundColor White
Write-Host "  4. Pour les endpoints authentifies:" -ForegroundColor White
Write-Host "     - D'abord, faire un POST /api/v1/auth/login" -ForegroundColor Gray
Write-Host "     - Copier le token recu" -ForegroundColor Gray
Write-Host "     - Cliquer sur 'Authorize' en haut de la page" -ForegroundColor Gray
Write-Host "     - Entrer 'Bearer TOKEN' dans le champ" -ForegroundColor Gray
Write-Host "     - Maintenant vous pouvez tester les endpoints proteges" -ForegroundColor Gray
Write-Host ""

Write-Host "AVANTAGES DE SWAGGER:" -ForegroundColor Yellow
Write-Host "  - Documentation toujours synchronisee avec le code" -ForegroundColor White
Write-Host "  - Interface utilisateur intuitive" -ForegroundColor White
Write-Host "  - Tests d'API sans outils externes" -ForegroundColor White
Write-Host "  - Generation automatique de la documentation" -ForegroundColor White
Write-Host "  - Support de l'authentification JWT" -ForegroundColor White
Write-Host ""

Write-Host "NOTE IMPORTANTE:" -ForegroundColor Red
Write-Host "  Swagger n'est disponible qu'en mode DEVELOPMENT" -ForegroundColor Yellow
Write-Host "  Pour l'activer, le service doit etre lance avec:" -ForegroundColor Yellow
Write-Host "    `$env:ASPNETCORE_ENVIRONMENT='Development'" -ForegroundColor Gray
Write-Host "    dotnet run" -ForegroundColor Gray
Write-Host ""

Write-Host "=====================================================" -ForegroundColor Green
Write-Host "    SWAGGER AUTH.API PRET A UTILISER!" -ForegroundColor Green
Write-Host "    Ouvrez votre navigateur sur: $swaggerHttpUrl" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green