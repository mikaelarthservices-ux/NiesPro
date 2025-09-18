using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace NiesPro.AdminPanel.Services;

/// <summary>
/// Service de notifications avec différents niveaux de priorité
/// </summary>
public interface INotificationService
{
    ObservableCollection<NotificationItem> Notifications { get; }
    void AddInfo(string title, string message);
    void AddWarning(string title, string message);
    void AddError(string title, string message);
    void AddSuccess(string title, string message);
    void ClearAll();
    void Remove(NotificationItem notification);
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
}

public class NotificationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    
    public ObservableCollection<NotificationItem> Notifications { get; private set; }

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
        Notifications = new ObservableCollection<NotificationItem>();
    }

    public void AddInfo(string title, string message)
    {
        AddNotification(title, message, NotificationType.Info);
    }

    public void AddWarning(string title, string message)
    {
        AddNotification(title, message, NotificationType.Warning);
    }

    public void AddError(string title, string message)
    {
        AddNotification(title, message, NotificationType.Error);
    }

    public void AddSuccess(string title, string message)
    {
        AddNotification(title, message, NotificationType.Success);
    }

    private void AddNotification(string title, string message, NotificationType type)
    {
        var notification = new NotificationItem
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.Now
        };

        // Ajouter sur le thread UI
        Application.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Insert(0, notification); // Ajouter en haut
            
            // Limiter à 100 notifications pour éviter la surcharge mémoire
            while (Notifications.Count > 100)
            {
                Notifications.RemoveAt(Notifications.Count - 1);
            }
        });

        _logger.LogInformation("Notification added: {Type} - {Title}: {Message}", 
            type, title, message);
    }

    public void ClearAll()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Clear();
        });
        
        _logger.LogInformation("All notifications cleared");
    }

    public void Remove(NotificationItem notification)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Remove(notification);
        });
    }
}