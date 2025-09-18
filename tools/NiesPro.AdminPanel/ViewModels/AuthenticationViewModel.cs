using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NiesPro.AdminPanel.Services;
using NiesPro.AdminPanel.Models;

namespace NiesPro.AdminPanel.ViewModels;

/// <summary>
/// ViewModel pour l'authentification avec validation
/// </summary>
public partial class AuthenticationViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AuthenticationViewModel> _logger;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    public AuthenticationViewModel(
        IAuthenticationService authService,
        INotificationService notificationService,
        ILogger<AuthenticationViewModel> logger)
    {
        _authService = authService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (!ValidateInput())
        {
            return;
        }

        try
        {
            IsLoading = true;
            ClearError();

            _logger.LogInformation("Attempting login for user: {Email}", Email);

            var result = await _authService.LoginAsync(Email, Password);

            if (result != null)
            {
                _logger.LogInformation("Login successful for user: {Email}", Email);
                _notificationService.AddSuccess("Authentification", "Connexion réussie");
                
                // Nettoyer les champs
                ClearFields();
            }
            else
            {
                ShowError("Email ou mot de passe incorrect");
                _logger.LogWarning("Login failed for user: {Email}", Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", Email);
            ShowError($"Erreur de connexion: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearFields()
    {
        Email = string.Empty;
        Password = string.Empty;
        RememberMe = false;
        ClearError();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("L'email est requis");
            return false;
        }

        if (!IsValidEmail(Email))
        {
            ShowError("Format d'email invalide");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Le mot de passe est requis");
            return false;
        }

        if (Password.Length < 6)
        {
            ShowError("Le mot de passe doit contenir au moins 6 caractères");
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
        _notificationService.AddError("Authentification", message);
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    partial void OnEmailChanged(string value)
    {
        if (HasError && !string.IsNullOrWhiteSpace(value))
        {
            ClearError();
        }
    }

    partial void OnPasswordChanged(string value)
    {
        if (HasError && !string.IsNullOrWhiteSpace(value))
        {
            ClearError();
        }
    }
}