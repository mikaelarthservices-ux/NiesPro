using CommunityToolkit.Mvvm.ComponentModel;

namespace NiesPro.AdminPanel.ViewModels;

/// <summary>
/// ViewModels pour les fonctionnalités métier - placeholders pour extension future
/// </summary>

public partial class UsersViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = string.Empty;

    // TODO: Implémenter la gestion des utilisateurs
    // - Liste des utilisateurs avec pagination
    // - CRUD utilisateurs
    // - Gestion des rôles et permissions
    // - Sessions actives et appareils
}

public partial class OrdersViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = string.Empty;

    // TODO: Implémenter la gestion des commandes
    // - Liste des commandes avec filtres
    // - Détails commande avec Event Sourcing
    // - Changement de statut
    // - Rapports et statistiques
}

public partial class ProductsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = string.Empty;

    // TODO: Implémenter la gestion du catalogue
    // - Hiérarchie des catégories
    // - CRUD produits avec images
    // - Gestion stock et variantes
    // - Import/Export en masse
}

public partial class PaymentsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = string.Empty;

    // TODO: Implémenter la gestion des paiements
    // - Transactions en temps réel
    // - Détection fraude
    // - Rapports financiers
    // - Méthodes de paiement
}