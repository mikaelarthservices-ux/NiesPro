using Microsoft.Extensions.DependencyInjection;
using NiesPro.AdminPanel.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace NiesPro.AdminPanel.Views;

/// <summary>
/// Fenêtre principale avec injection de dépendances
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }

    private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            // Annuler la fermeture temporairement pour permettre le cleanup
            e.Cancel = true;
            
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.CleanupAsync();
            }
            
            // Attendre un peu pour s'assurer que tout est arrêté
            await Task.Delay(500);
            
            // Force la fermeture de l'application
            Application.Current.Shutdown();
        }
        catch (Exception)
        {
            // En cas d'erreur, force la fermeture immédiatement
            Application.Current.Shutdown(1);
        }
    }
}