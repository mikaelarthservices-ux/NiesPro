# PowerShell script pour créer les bases NiesPro via WAMP/MySQL
# Utilise l'API REST ou les commandes MySQL disponibles

Write-Host "🚀 Création des bases de données NiesPro via WAMP..." -ForegroundColor Green

# Chemins WAMP typiques
$wampPaths = @(
    "C:\wamp64\bin\mysql\mysql8.0.31\bin\mysql.exe",
    "C:\wamp\bin\mysql\mysql8.0.31\bin\mysql.exe",
    "C:\wamp64\bin\mysql\mysql5.7.36\bin\mysql.exe",
    "C:\wamp\bin\mysql\mysql5.7.36\bin\mysql.exe"
)

# Trouver MySQL
$mysqlPath = $null
foreach ($path in $wampPaths) {
    if (Test-Path $path) {
        $mysqlPath = $path
        Write-Host "✅ MySQL trouvé : $path" -ForegroundColor Green
        break
    }
}

if (-not $mysqlPath) {
    Write-Host "❌ MySQL non trouvé dans les chemins WAMP standard" -ForegroundColor Red
    Write-Host "📋 Solutions alternatives :" -ForegroundColor Yellow
    Write-Host "1. Utiliser phpMyAdmin : http://localhost/phpmyadmin/" -ForegroundColor Cyan
    Write-Host "2. Localiser manuellement mysql.exe dans WAMP" -ForegroundColor Cyan
    exit 1
}

# Exécuter le script SQL
$scriptPath = "C:\Users\HP\Documents\projets\NiesPro\scripts\database\create_databases_wamp.sql"

Write-Host "📄 Exécution du script : $scriptPath" -ForegroundColor Blue

try {
    & $mysqlPath -u root -e "source $scriptPath"
    Write-Host "✅ Bases de données créées avec succès !" -ForegroundColor Green
    
    # Vérifier les bases créées
    Write-Host "🔍 Vérification des bases créées :" -ForegroundColor Blue
    & $mysqlPath -u root -e "SHOW DATABASES LIKE 'NiesPro_%';"
    
} catch {
    Write-Host "❌ Erreur lors de la création : $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "🌐 Utilisez phpMyAdmin : http://localhost/phpmyadmin/" -ForegroundColor Yellow
}

Write-Host "🎯 Script terminé." -ForegroundColor Green