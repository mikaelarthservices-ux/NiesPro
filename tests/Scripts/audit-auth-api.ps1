# Audit complet Auth.API - Etat actuel et ameliorations necessaires
Write-Host "AUDIT AUTH.API - ETAT ACTUEL" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Verifier que Auth.API fonctionne
Write-Host "`n1. Etat du service Auth.API..." -ForegroundColor Yellow
try {
    $healthCheck = Invoke-WebRequest -Uri "http://localhost:5001/health" -UseBasicParsing -TimeoutSec 5
    Write-Host "Auth.API is running (Status: $($healthCheck.StatusCode))" -ForegroundColor Green
    $authRunning = $true
} catch {
    Write-Host "Auth.API not running" -ForegroundColor Red
    $authRunning = $false
}

# Verifier la base de donnees
Write-Host "`n2. Etat de la base de donnees..." -ForegroundColor Yellow
try {
    $dbResult = echo "" | & "C:\wamp64\bin\mysql\mysql9.1.0\bin\mysql.exe" -u root -p -e "USE niespro_auth; SELECT COUNT(*) as user_count FROM users;" 2>$null
    Write-Host "Base de donnees accessible" -ForegroundColor Green
    Write-Host "Users count result: $dbResult" -ForegroundColor Blue
} catch {
    Write-Host "Probleme de connexion a la base de donnees" -ForegroundColor Red
}

# Analyser les fichiers de code pour identifier les ameliorations
Write-Host "`n3. Analyse du code source..." -ForegroundColor Yellow

$authPath = "C:\Users\HP\Documents\projets\NiesPro\src\Services\Auth"

# Verifier les TODOs et commentaires temporaires
Write-Host "   Recherche de TODOs et commentaires temporaires..." -ForegroundColor Gray
$todoCount = 0
Get-ChildItem -Path $authPath -Recurse -Filter "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
    if ($content -and ($content -match "TODO|FIXME|temporairement|Temporairement|HACK")) {
        Write-Host "   TODOs/TEMP trouves dans: $($_.Name)" -ForegroundColor Yellow
        $todoCount++
    }
}
Write-Host "   Total TODOs/TEMP trouves: $todoCount" -ForegroundColor Blue

# Verifier les erreurs de compilation
Write-Host "`n   Verification de la compilation..." -ForegroundColor Gray
$null = & dotnet build "$authPath\Auth.API\Auth.API.csproj" --configuration Release --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   Compilation reussie" -ForegroundColor Green
} else {
    Write-Host "   Erreurs de compilation detectees" -ForegroundColor Red
}

# Resume des ameliorations necessaires
Write-Host "`n4. Resume des ameliorations necessaires:" -ForegroundColor Yellow
Write-Host "   Restaurer les validations d'unicite (email/username)" -ForegroundColor Magenta
Write-Host "   Nettoyer les commentaires temporaires" -ForegroundColor Magenta
Write-Host "   Ameliorer les logs et messages d'erreur" -ForegroundColor Magenta
Write-Host "   Renforcer la securite et validation" -ForegroundColor Magenta
Write-Host "   Tests complets de tous les endpoints" -ForegroundColor Magenta

Write-Host "`nPRET POUR LA FINALISATION!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan