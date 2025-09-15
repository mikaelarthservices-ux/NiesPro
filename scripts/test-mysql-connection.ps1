# Script de test de connexion MySQL pour NiesPro
param(
    [string]$Host = "localhost",
    [string]$Port = "3306",
    [string]$User = "niespro",
    [string]$Password = "NiesPro2025!"
)

Write-Host "🔍 Test de connexion MySQL NiesPro" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Vérifier si MySQL est accessible
Write-Host "📡 Test de connectivité sur $Host`:$Port..." -ForegroundColor Yellow

try {
    $connection = Test-NetConnection -ComputerName $Host -Port $Port -WarningAction SilentlyContinue
    if ($connection.TcpTestSucceeded) {
        Write-Host "✅ MySQL est accessible sur le port $Port" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL n'est pas accessible sur le port $Port" -ForegroundColor Red
        Write-Host "💡 Vérifiez que MySQL Server est installé et démarré" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "❌ Erreur lors du test de connectivité: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Tester avec mysql.exe si disponible
Write-Host "🔐 Test de connexion avec utilisateur $User..." -ForegroundColor Yellow

$mysqlCommand = "mysql"
$mysqlPath = Get-Command mysql -ErrorAction SilentlyContinue

if ($mysqlPath) {
    Write-Host "✅ Client MySQL trouvé: $($mysqlPath.Source)" -ForegroundColor Green
    
    # Tester la connexion
    $testQuery = "SELECT 'Connexion réussie!' AS Status, NOW() AS Timestamp;"
    
    try {
        $result = & mysql -h $Host -P $Port -u $User -p$Password -e $testQuery 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Connexion MySQL réussie!" -ForegroundColor Green
            Write-Host "📊 Résultat du test:" -ForegroundColor Cyan
            Write-Host $result -ForegroundColor White
        } else {
            Write-Host "❌ Échec de connexion MySQL" -ForegroundColor Red
            Write-Host "💡 Vérifiez les identifiants: $User / [password]" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "❌ Erreur lors de la connexion: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️ Client mysql.exe non trouvé dans le PATH" -ForegroundColor Yellow
    Write-Host "💡 Installez MySQL Server ou ajoutez-le au PATH" -ForegroundColor Yellow
}

# Lister les bases NiesPro si la connexion fonctionne
if ($LASTEXITCODE -eq 0) {
    Write-Host "🗄️ Vérification des bases NiesPro..." -ForegroundColor Yellow
    
    $dbQuery = "SHOW DATABASES LIKE 'NiesPro_%';"
    try {
        $databases = & mysql -h $Host -P $Port -u $User -p$Password -e $dbQuery 2>$null
        if ($databases) {
            Write-Host "✅ Bases NiesPro trouvées:" -ForegroundColor Green
            Write-Host $databases -ForegroundColor White
        } else {
            Write-Host "⚠️ Aucune base NiesPro trouvée" -ForegroundColor Yellow
            Write-Host "💡 Exécutez le script create_real_databases.sql" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "⚠️ Impossible de lister les bases de données" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "📋 Résumé de la configuration:" -ForegroundColor Cyan
Write-Host "   Host: $Host" -ForegroundColor White
Write-Host "   Port: $Port" -ForegroundColor White
Write-Host "   User: $User" -ForegroundColor White
Write-Host "   Password: [caché]" -ForegroundColor White
Write-Host ""

if ($LASTEXITCODE -eq 0) {
    Write-Host "🎉 Configuration MySQL prête pour NiesPro!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Prochaines étapes:" -ForegroundColor Cyan
    Write-Host "   1. Exécuter les migrations Entity Framework" -ForegroundColor White
    Write-Host "   2. Démarrer l'API Auth" -ForegroundColor White
    Write-Host "   3. Tester l'authentification" -ForegroundColor White
} else {
    Write-Host "❌ Configuration MySQL incomplète" -ForegroundColor Red
    Write-Host ""
    Write-Host "📝 Actions requises:" -ForegroundColor Cyan
    Write-Host "   1. Installer MySQL Server 8.0" -ForegroundColor White
    Write-Host "   2. Exécuter le script create_real_databases.sql" -ForegroundColor White
    Write-Host "   3. Relancer ce test" -ForegroundColor White
}