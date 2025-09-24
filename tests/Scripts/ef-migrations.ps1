# =================================================================
# GESTIONNAIRE DE MIGRATIONS EF - NIESPRO ERP
# =================================================================
# Description: Outil pour gérer les migrations Entity Framework
# Fonctionnalités:
# - Application automatique des migrations
# - Vérification de l'état des migrations
# - Création de nouvelles migrations
# - Rollback des migrations
# =================================================================

param(
    [ValidateSet("apply", "check", "create", "rollback", "reset")]
    [string]$Action = "check",
    [string]$Service = "all",
    [string]$MigrationName = "",
    [string]$TargetMigration = "",
    [switch]$Force,
    [switch]$DryRun
)

# Configuration des services avec Entity Framework
$workspaceRoot = "C:\Users\HP\Documents\projets\NiesPro"
$efServices = @{
    "auth" = @{
        Name = "Auth.API"
        Path = "$workspaceRoot\src\Services\Auth\Auth.API"
        ProjectFile = "Auth.API.csproj"
        DbContext = "AuthDbContext"
        Database = "NiesPro_Auth"
    }
    "customer" = @{
        Name = "Customer.API"
        Path = "$workspaceRoot\src\Services\Customer.API"
        ProjectFile = "Customer.API.csproj"
        DbContext = "CustomerDbContext"
        Database = "NiesPro_Customer"
    }
    "catalog" = @{
        Name = "Catalog.API"
        Path = "$workspaceRoot\src\Services\Catalog\Catalog.API"
        ProjectFile = "Catalog.API.csproj"
        DbContext = "CatalogDbContext"
        Database = "NiesPro_Catalog"
    }
    "order" = @{
        Name = "Order.API"
        Path = "$workspaceRoot\src\Services\Order\Order.API"
        ProjectFile = "Order.API.csproj"
        DbContext = "OrderDbContext"
        Database = "NiesPro_Order"
    }
    "payment" = @{
        Name = "Payment.API"
        Path = "$workspaceRoot\src\Services\Payment\Payment.API"
        ProjectFile = "Payment.API.csproj"
        DbContext = "PaymentDbContext"
        Database = "NiesPro_Payment"
    }
    "stock" = @{
        Name = "Stock.API"
        Path = "$workspaceRoot\src\Services\Stock\Stock.API"
        ProjectFile = "Stock.API.csproj"
        DbContext = "StockDbContext"
        Database = "NiesPro_Stock"
    }
    "restaurant" = @{
        Name = "Restaurant.API"
        Path = "$workspaceRoot\src\Services\Restaurant\Restaurant.API"
        ProjectFile = "Restaurant.API.csproj"
        DbContext = "RestaurantDbContext"
        Database = "NiesPro_Restaurant"
    }
}

# Fonction pour vérifier l'installation d'EF Core Tools
function Test-EFTools {
    try {
        $efVersion = dotnet ef --version 2>$null
        if ($efVersion) {
            Write-Host "✓ EF Core Tools installé: $efVersion" -ForegroundColor Green
            return $true
        }
        return $false
    }
    catch {
        return $false
    }
}

# Fonction pour installer EF Core Tools
function Install-EFTools {
    Write-Host "Installation d'EF Core Tools..." -ForegroundColor Yellow
    try {
        dotnet tool install --global dotnet-ef
        Write-Host "✓ EF Core Tools installé avec succès" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ Échec de l'installation d'EF Core Tools" -ForegroundColor Red
        return $false
    }
}

# Fonction pour obtenir la liste des migrations
function Get-Migrations {
    param($ServiceConfig, $ServiceKey)
    
    $originalPath = Get-Location
    try {
        Set-Location $ServiceConfig.Path
        
        Write-Host "🔍 Vérification des migrations pour $($ServiceConfig.Name)..." -ForegroundColor Cyan
        
        # Vérifier si le projet existe
        if (-not (Test-Path $ServiceConfig.ProjectFile)) {
            Write-Host "   ⚠ Projet non trouvé: $($ServiceConfig.ProjectFile)" -ForegroundColor Yellow
            return @{ Applied = @(); Pending = @(); Status = "ProjectNotFound" }
        }
        
        # Obtenir les migrations appliquées
        $appliedMigrations = @()
        try {
            $appliedOutput = dotnet ef migrations list --context $ServiceConfig.DbContext --project $ServiceConfig.ProjectFile 2>$null
            if ($appliedOutput) {
                $appliedMigrations = $appliedOutput | Where-Object { $_ -and $_ -notmatch "^(Build started|Build succeeded|info:|warn:)" }
            }
        }
        catch {
            Write-Host "   ⚠ Impossible de récupérer les migrations appliquées" -ForegroundColor Yellow
        }
        
        # Obtenir les migrations en attente
        $pendingMigrations = @()
        try {
            $pendingOutput = dotnet ef migrations has-pending-model-changes --context $ServiceConfig.DbContext --project $ServiceConfig.ProjectFile 2>$null
            $hasPending = $pendingOutput -match "true"
        }
        catch {
            $hasPending = $false
        }
        
        $status = if ($appliedMigrations.Count -gt 0) { "Ready" } else { "NeedsInitialization" }
        
        return @{
            Applied = $appliedMigrations
            Pending = $hasPending
            Status = $status
        }
    }
    catch {
        Write-Host "   ✗ Erreur lors de la vérification des migrations: $_" -ForegroundColor Red
        return @{ Applied = @(); Pending = @(); Status = "Error" }
    }
    finally {
        Set-Location $originalPath
    }
}

# Fonction pour appliquer les migrations
function Invoke-Migrations {
    param($ServiceConfig, $ServiceKey)
    
    $originalPath = Get-Location
    try {
        Set-Location $ServiceConfig.Path
        
        Write-Host "🚀 Application des migrations pour $($ServiceConfig.Name)..." -ForegroundColor Green
        
        if ($DryRun) {
            Write-Host "   [DRY RUN] Simulation de: dotnet ef database update" -ForegroundColor Yellow
            return $true
        }
        
        # Appliquer les migrations
        $updateResult = dotnet ef database update --context $ServiceConfig.DbContext --project $ServiceConfig.ProjectFile 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✓ Migrations appliquées avec succès" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ✗ Échec de l'application des migrations:" -ForegroundColor Red
            $updateResult | ForEach-Object { Write-Host "     $_" -ForegroundColor Red }
            return $false
        }
    }
    catch {
        Write-Host "   ✗ Erreur lors de l'application des migrations: $_" -ForegroundColor Red
        return $false
    }
    finally {
        Set-Location $originalPath
    }
}

# Fonction pour créer une nouvelle migration
function New-Migration {
    param($ServiceConfig, $ServiceKey, $MigrationName)
    
    $originalPath = Get-Location
    try {
        Set-Location $ServiceConfig.Path
        
        Write-Host "📝 Création de la migration '$MigrationName' pour $($ServiceConfig.Name)..." -ForegroundColor Cyan
        
        if ($DryRun) {
            Write-Host "   [DRY RUN] Simulation de: dotnet ef migrations add $MigrationName" -ForegroundColor Yellow
            return $true
        }
        
        # Créer la migration
        $addResult = dotnet ef migrations add $MigrationName --context $ServiceConfig.DbContext --project $ServiceConfig.ProjectFile 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✓ Migration créée avec succès" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ✗ Échec de la création de la migration:" -ForegroundColor Red
            $addResult | ForEach-Object { Write-Host "     $_" -ForegroundColor Red }
            return $false
        }
    }
    catch {
        Write-Host "   ✗ Erreur lors de la création de la migration: $_" -ForegroundColor Red
        return $false
    }
    finally {
        Set-Location $originalPath
    }
}

# Fonction pour effectuer un rollback
function Invoke-Rollback {
    param($ServiceConfig, $ServiceKey, $TargetMigration)
    
    $originalPath = Get-Location
    try {
        Set-Location $ServiceConfig.Path
        
        Write-Host "↩️ Rollback vers '$TargetMigration' pour $($ServiceConfig.Name)..." -ForegroundColor Yellow
        
        if ($DryRun) {
            Write-Host "   [DRY RUN] Simulation de: dotnet ef database update $TargetMigration" -ForegroundColor Yellow
            return $true
        }
        
        # Confirmer l'action si pas de force
        if (-not $Force) {
            $confirm = Read-Host "   Confirmer le rollback vers '$TargetMigration'? (y/N)"
            if ($confirm -ne 'y' -and $confirm -ne 'Y') {
                Write-Host "   Rollback annulé" -ForegroundColor Gray
                return $false
            }
        }
        
        # Effectuer le rollback
        $rollbackResult = dotnet ef database update $TargetMigration --context $ServiceConfig.DbContext --project $ServiceConfig.ProjectFile 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✓ Rollback effectué avec succès" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ✗ Échec du rollback:" -ForegroundColor Red
            $rollbackResult | ForEach-Object { Write-Host "     $_" -ForegroundColor Red }
            return $false
        }
    }
    catch {
        Write-Host "   ✗ Erreur lors du rollback: $_" -ForegroundColor Red
        return $false
    }
    finally {
        Set-Location $originalPath
    }
}

# Fonction principale
function Start-MigrationManager {
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "GESTIONNAIRE DE MIGRATIONS EF - NIESPRO ERP" -ForegroundColor Cyan
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "Action: $Action" -ForegroundColor Gray
    Write-Host "Service: $Service" -ForegroundColor Gray
    Write-Host "Heure: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""
    
    # Vérifier EF Core Tools
    if (-not (Test-EFTools)) {
        Write-Host "EF Core Tools non installé" -ForegroundColor Red
        if ($Force) {
            if (-not (Install-EFTools)) {
                return
            }
        } else {
            Write-Host "Utilisez --Force pour installer automatiquement" -ForegroundColor Yellow
            return
        }
    }
    
    # Déterminer les services à traiter
    $servicesToProcess = @()
    if ($Service -eq "all") {
        $servicesToProcess = $efServices.Keys
    } elseif ($efServices.ContainsKey($Service)) {
        $servicesToProcess = @($Service)
    } else {
        Write-Host "Service inconnu: $Service" -ForegroundColor Red
        Write-Host "Services disponibles: $($efServices.Keys -join ', ')" -ForegroundColor Yellow
        return
    }
    
    $successCount = 0
    $totalCount = $servicesToProcess.Count
    
    # Traitement selon l'action
    foreach ($serviceKey in $servicesToProcess) {
        $serviceConfig = $efServices[$serviceKey]
        
        switch ($Action) {
            "check" {
                $migrationInfo = Get-Migrations -ServiceConfig $serviceConfig -ServiceKey $serviceKey
                
                Write-Host "$($serviceConfig.Name):" -ForegroundColor Cyan
                Write-Host "  Status: $($migrationInfo.Status)" -ForegroundColor Gray
                Write-Host "  Migrations appliquées: $($migrationInfo.Applied.Count)" -ForegroundColor Gray
                Write-Host "  Changements en attente: $($migrationInfo.Pending)" -ForegroundColor Gray
                
                if ($migrationInfo.Status -eq "Ready") {
                    $successCount++
                }
                Write-Host ""
            }
            
            "apply" {
                $success = Invoke-Migrations -ServiceConfig $serviceConfig -ServiceKey $serviceKey
                if ($success) { $successCount++ }
                Write-Host ""
            }
            
            "create" {
                if (-not $MigrationName) {
                    Write-Host "Nom de migration requis pour l'action 'create'" -ForegroundColor Red
                    continue
                }
                $success = New-Migration -ServiceConfig $serviceConfig -ServiceKey $serviceKey -MigrationName $MigrationName
                if ($success) { $successCount++ }
                Write-Host ""
            }
            
            "rollback" {
                if (-not $TargetMigration) {
                    Write-Host "Migration cible requise pour l'action 'rollback'" -ForegroundColor Red
                    continue
                }
                $success = Invoke-Rollback -ServiceConfig $serviceConfig -ServiceKey $serviceKey -TargetMigration $TargetMigration
                if ($success) { $successCount++ }
                Write-Host ""
            }
            
            "reset" {
                if ($Force -or (Read-Host "Réinitialiser toutes les migrations pour $($serviceConfig.Name)? (y/N)") -eq 'y') {
                    Write-Host "🔄 Réinitialisation des migrations pour $($serviceConfig.Name)..." -ForegroundColor Yellow
                    # Implementation de reset si nécessaire
                    Write-Host "   ⚠ Fonctionnalité en développement" -ForegroundColor Yellow
                }
                Write-Host ""
            }
        }
    }
    
    # Résumé
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "Résumé: $successCount/$totalCount services traités avec succès" -ForegroundColor Green
    Write-Host "===============================================" -ForegroundColor Cyan
}

# Exécution
Start-MigrationManager