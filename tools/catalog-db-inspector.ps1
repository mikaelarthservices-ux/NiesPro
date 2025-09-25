# =======================================================================
# CATALOG DATABASE INSPECTOR - NiesPro
# Script pour consulter et gérer la base de données Catalog
# =======================================================================

param(
    [string]$Action = "menu",
    [string]$ConnectionString = "",
    [string]$ServerHost = "localhost",
    [string]$Port = "3306", 
    [string]$User = "root",
    [string]$Password = "",
    [string]$Database = "niespro_catalog"
)

Write-Host "🔍 CATALOG DATABASE INSPECTOR" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host "Base de données: $Database sur $ServerHost`:$Port" -ForegroundColor Cyan
Write-Host ""

# Construction de la chaîne de connexion si pas fournie
if (-not $ConnectionString) {
    if ($Password) {
        $ConnectionString = "Server=$ServerHost;Port=$Port;Database=$Database;Uid=$User;Pwd=$Password;"
    } else {
        $ConnectionString = "Server=$ServerHost;Port=$Port;Database=$Database;Uid=$User;"
    }
}

# Fonction pour exécuter une requête MySQL
function Invoke-MySqlQuery {
    param([string]$Query, [switch]$ShowResults = $true)
    
    try {
        # Essayer avec mysql.exe si disponible
        $mysqlPaths = @(
            "mysql",
            "C:\wamp64\bin\mysql\mysql9.1.0\bin\mysql.exe",
            "C:\wamp64\bin\mariadb\mariadb11.5.2\bin\mysql.exe",
            "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
            "C:\wamp64\bin\mysql\mysql8.0.31\bin\mysql.exe",
            "C:\xampp\mysql\bin\mysql.exe"
        )
        
        $mysqlCmd = $null
        foreach ($path in $mysqlPaths) {
            $cmd = Get-Command $path -ErrorAction SilentlyContinue
            if ($cmd) {
                $mysqlCmd = $cmd.Source
                break
            }
        }
        
        if ($mysqlCmd) {
            $tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
            $Query | Out-File -FilePath $tempFile -Encoding UTF8
            
            if ($Password) {
                $result = & $mysqlCmd -h $ServerHost -P $Port -u $User -p$Password $Database -e "source $tempFile" 2>&1
            } else {
                $result = & $mysqlCmd -h $ServerHost -P $Port -u $User $Database -e "source $tempFile" 2>&1
            }
            
            Remove-Item $tempFile -Force
            
            if ($ShowResults -and $result) {
                Write-Host $result -ForegroundColor White
            }
            
            return $result
        } else {
            Write-Host "❌ Client MySQL non trouvé" -ForegroundColor Red
            Write-Host "💡 Utilisez phpMyAdmin: http://localhost/phpmyadmin/" -ForegroundColor Yellow
            return $null
        }
    }
    catch {
        Write-Host "❌ Erreur: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Fonction pour tester la connexion
function Test-DatabaseConnection {
    Write-Host "🔌 Test de connexion à la base..." -ForegroundColor Yellow
    
    $testQuery = "SELECT 'Connexion réussie!' as Status, NOW() as Timestamp, DATABASE() as CurrentDB;"
    $result = Invoke-MySqlQuery -Query $testQuery
    
    if ($result) {
        Write-Host "✅ Connexion réussie!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ Échec de connexion" -ForegroundColor Red
        return $false
    }
}

# Fonction pour lister les tables
function Show-Tables {
    Write-Host "📋 Tables de la base $Database :" -ForegroundColor Yellow
    $query = "SHOW TABLES;"
    Invoke-MySqlQuery -Query $query
}

# Fonction pour afficher la structure d'une table
function Show-TableStructure {
    param([string]$TableName)
    
    Write-Host "🏗️  Structure de la table '$TableName' :" -ForegroundColor Yellow
    $query = "DESCRIBE $TableName;"
    Invoke-MySqlQuery -Query $query
}

# Fonction pour compter les enregistrements
function Show-RecordCounts {
    Write-Host "📊 Nombre d'enregistrements par table :" -ForegroundColor Yellow
    
    $tables = @("Products", "Categories", "Brands", "ProductVariants", "ProductAttributes", "Reviews")
    
    foreach ($table in $tables) {
        $query = "SELECT COUNT(*) as Count FROM $table;"
        Write-Host "Table $table :" -ForegroundColor Cyan -NoNewline
        $result = Invoke-MySqlQuery -Query $query -ShowResults:$false
        if ($result) {
            Write-Host " $result" -ForegroundColor White
        } else {
            Write-Host " [Table n'existe pas]" -ForegroundColor Gray
        }
    }
}

# Fonction pour afficher les données d'exemple
function Show-SampleData {
    param([string]$TableName, [int]$Limit = 5)
    
    Write-Host "📄 Données d'exemple de la table '$TableName' (limite: $Limit) :" -ForegroundColor Yellow
    $query = "SELECT * FROM $TableName LIMIT $Limit;"
    Invoke-MySqlQuery -Query $query
}

# Fonction pour afficher les catégories
function Show-Categories {
    Write-Host "🗂️  Catégories :" -ForegroundColor Yellow
    $query = @"
SELECT 
    Id, 
    Name, 
    Slug, 
    IsActive,
    ParentCategoryId,
    CreatedAt 
FROM Categories 
ORDER BY SortOrder, Name;
"@
    Invoke-MySqlQuery -Query $query
}

# Fonction pour afficher les produits
function Show-Products {
    Write-Host "📦 Produits :" -ForegroundColor Yellow
    $query = @"
SELECT 
    p.Id,
    p.Name,
    p.SKU,
    p.Price,
    p.IsActive,
    p.Quantity,
    c.Name as CategoryName
FROM Products p
LEFT JOIN Categories c ON p.CategoryId = c.Id
ORDER BY p.CreatedAt DESC
LIMIT 10;
"@
    Invoke-MySqlQuery -Query $query
}

# Fonction pour vérifier l'intégrité des données
function Test-DataIntegrity {
    Write-Host "🔍 Vérification de l'intégrité des données :" -ForegroundColor Yellow
    
    # Vérifier les produits sans catégorie
    Write-Host "Produits sans catégorie valide :" -ForegroundColor Cyan
    $query1 = @"
SELECT COUNT(*) as Count 
FROM Products p 
LEFT JOIN Categories c ON p.CategoryId = c.Id 
WHERE p.CategoryId IS NOT NULL AND c.Id IS NULL;
"@
    Invoke-MySqlQuery -Query $query1
    
    # Vérifier les SKU dupliqués
    Write-Host "SKU dupliqués :" -ForegroundColor Cyan
    $query2 = @"
SELECT SKU, COUNT(*) as Count 
FROM Products 
GROUP BY SKU 
HAVING COUNT(*) > 1;
"@
    Invoke-MySqlQuery -Query $query2
}

# Menu principal
function Show-Menu {
    Write-Host "🎯 Choisissez une action :" -ForegroundColor Yellow
    Write-Host "1. Tester la connexion" -ForegroundColor White
    Write-Host "2. Lister les tables" -ForegroundColor White
    Write-Host "3. Compter les enregistrements" -ForegroundColor White
    Write-Host "4. Afficher les catégories" -ForegroundColor White
    Write-Host "5. Afficher les produits" -ForegroundColor White
    Write-Host "6. Structure d'une table" -ForegroundColor White
    Write-Host "7. Données d'exemple" -ForegroundColor White
    Write-Host "8. Vérifier l'intégrité" -ForegroundColor White
    Write-Host "9. Requête personnalisée" -ForegroundColor White
    Write-Host "0. Quitter" -ForegroundColor White
    Write-Host ""
}

# Exécution principale
switch ($Action.ToLower()) {
    "menu" {
        do {
            Show-Menu
            $choice = Read-Host "Votre choix"
            
            switch ($choice) {
                "1" { Test-DatabaseConnection }
                "2" { Show-Tables }
                "3" { Show-RecordCounts }
                "4" { Show-Categories }
                "5" { Show-Products }
                "6" { 
                    $table = Read-Host "Nom de la table"
                    Show-TableStructure -TableName $table
                }
                "7" { 
                    $table = Read-Host "Nom de la table"
                    $limit = Read-Host "Nombre de lignes (défaut: 5)"
                    if (-not $limit) { $limit = 5 }
                    Show-SampleData -TableName $table -Limit $limit
                }
                "8" { Test-DataIntegrity }
                "9" { 
                    $customQuery = Read-Host "Entrez votre requête SQL"
                    Invoke-MySqlQuery -Query $customQuery
                }
                "0" { 
                    Write-Host "👋 Au revoir!" -ForegroundColor Green
                    break 
                }
                default { 
                    Write-Host "❌ Choix invalide" -ForegroundColor Red 
                }
            }
            
            if ($choice -ne "0") {
                Write-Host ""
                Read-Host "Appuyez sur Entrée pour continuer"
                Clear-Host
                Write-Host "🔍 CATALOG DATABASE INSPECTOR" -ForegroundColor Green
                Write-Host "============================" -ForegroundColor Green
            }
        } while ($choice -ne "0")
    }
    "test" { Test-DatabaseConnection }
    "tables" { Show-Tables }
    "counts" { Show-RecordCounts }
    "categories" { Show-Categories }
    "products" { Show-Products }
    "integrity" { Test-DataIntegrity }
    default {
        Write-Host "❌ Action non reconnue: $Action" -ForegroundColor Red
        Write-Host "Actions disponibles: menu, test, tables, counts, categories, products, integrity" -ForegroundColor Yellow
    }
}