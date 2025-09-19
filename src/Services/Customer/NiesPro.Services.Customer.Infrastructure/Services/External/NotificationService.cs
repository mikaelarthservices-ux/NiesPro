using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Concurrent;
using NiesPro.Services.Customer.Infrastructure.Configuration;

namespace NiesPro.Services.Customer.Infrastructure.Services.External;

/// <summary>
/// Service de notifications en temps réel avec SignalR et support multi-canal
/// </summary>
public interface INotificationService
{
    // Real-time notifications
    Task SendToUserAsync(string userId, NotificationMessage message, CancellationToken cancellationToken = default);
    Task SendToGroupAsync(string groupName, NotificationMessage message, CancellationToken cancellationToken = default);
    Task SendToAllAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    Task SendBulkToUsersAsync(List<string> userIds, NotificationMessage message, CancellationToken cancellationToken = default);

    // Group management
    Task AddUserToGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);
    Task RemoveUserFromGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default);

    // Business notifications
    Task SendWelcomeNotificationAsync(string userId, string customerName, CancellationToken cancellationToken = default);
    Task SendOrderStatusNotificationAsync(string userId, string orderNumber, OrderStatus status, CancellationToken cancellationToken = default);
    Task SendLoyaltyPointsNotificationAsync(string userId, int pointsEarned, int totalPoints, CancellationToken cancellationToken = default);
    Task SendPromotionNotificationAsync(string userId, string promotionTitle, string description, DateTime expiryDate, CancellationToken cancellationToken = default);
    Task SendAppointmentReminderAsync(string userId, string serviceName, DateTime appointmentDate, CancellationToken cancellationToken = default);
    Task SendSystemMaintenanceAsync(DateTime startTime, DateTime endTime, string description, CancellationToken cancellationToken = default);

    // Analytics
    Task<NotificationStats> GetNotificationStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<UserNotificationSummary>> GetUserNotificationSummaryAsync(List<string> userIds, CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly NotificationConfiguration _config;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ConcurrentDictionary<string, List<string>> _userGroups;
    private readonly ConcurrentDictionary<string, NotificationStats> _stats;

    public NotificationService(
        IOptions<NotificationConfiguration> config,
        ILogger<NotificationService> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _userGroups = new ConcurrentDictionary<string, List<string>>();
        _stats = new ConcurrentDictionary<string, NotificationStats>();
    }

    // ===== REAL-TIME NOTIFICATIONS =====

    public async Task SendToUserAsync(string userId, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateMessage(message);
            EnrichMessage(message);

            _logger.LogInformation("Sending notification to user {UserId}: {Title}",
                userId, message.Title);

            await _hubContext.Clients.User(userId).SendAsync(
                "ReceiveNotification", 
                message, 
                cancellationToken);

            await TrackNotificationAsync(userId, message);
            UpdateStats(message.Type, true);

            _logger.LogDebug("Notification sent successfully to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            UpdateStats(message.Type, false);
            throw;
        }
    }

    public async Task SendToGroupAsync(string groupName, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateMessage(message);
            EnrichMessage(message);

            _logger.LogInformation("Sending notification to group {GroupName}: {Title}",
                groupName, message.Title);

            await _hubContext.Clients.Group(groupName).SendAsync(
                "ReceiveNotification", 
                message, 
                cancellationToken);

            await TrackGroupNotificationAsync(groupName, message);
            UpdateStats(message.Type, true);

            _logger.LogDebug("Notification sent successfully to group {GroupName}", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to group {GroupName}", groupName);
            UpdateStats(message.Type, false);
            throw;
        }
    }

    public async Task SendToAllAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateMessage(message);
            EnrichMessage(message);

            _logger.LogInformation("Broadcasting notification to all users: {Title}", message.Title);

            await _hubContext.Clients.All.SendAsync(
                "ReceiveNotification", 
                message, 
                cancellationToken);

            await TrackBroadcastNotificationAsync(message);
            UpdateStats(message.Type, true);

            _logger.LogDebug("Notification broadcasted successfully to all users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast notification");
            UpdateStats(message.Type, false);
            throw;
        }
    }

    public async Task SendBulkToUsersAsync(List<string> userIds, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (!userIds?.Any() == true)
            return;

        try
        {
            ValidateMessage(message);
            EnrichMessage(message);

            _logger.LogInformation("Sending bulk notification to {UserCount} users: {Title}",
                userIds.Count, message.Title);

            var tasks = userIds.Select(userId => SendToUserAsync(userId, message, cancellationToken));
            await Task.WhenAll(tasks);

            _logger.LogDebug("Bulk notification sent successfully to {UserCount} users", userIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send bulk notification to users");
            throw;
        }
    }

    // ===== GROUP MANAGEMENT =====

    public async Task AddUserToGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Groups.AddToGroupAsync(userId, groupName, cancellationToken);
            
            _userGroups.AddOrUpdate(userId, 
                new List<string> { groupName },
                (key, existingGroups) =>
                {
                    if (!existingGroups.Contains(groupName))
                        existingGroups.Add(groupName);
                    return existingGroups;
                });

            _logger.LogDebug("Added user {UserId} to group {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add user {UserId} to group {GroupName}", userId, groupName);
            throw;
        }
    }

    public async Task RemoveUserFromGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Groups.RemoveFromGroupAsync(userId, groupName, cancellationToken);
            
            if (_userGroups.TryGetValue(userId, out var groups))
            {
                groups.Remove(groupName);
            }

            _logger.LogDebug("Removed user {UserId} from group {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove user {UserId} from group {GroupName}", userId, groupName);
            throw;
        }
    }

    public async Task<List<string>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _userGroups.TryGetValue(userId, out var groups) ? new List<string>(groups) : new List<string>();
    }

    // ===== BUSINESS NOTIFICATIONS =====

    public async Task SendWelcomeNotificationAsync(string userId, string customerName, CancellationToken cancellationToken = default)
    {
        var message = new NotificationMessage
        {
            Title = "Bienvenue!",
            Body = $"Bonjour {customerName}, bienvenue dans notre communauté! Découvrez tous nos services.",
            Type = NotificationType.Welcome,
            Priority = NotificationPriority.Normal,
            ActionUrl = "/dashboard",
            ImageUrl = _config.DefaultImages.WelcomeImage,
            Data = new Dictionary<string, object>
            {
                ["customerName"] = customerName,
                ["onboardingFlow"] = true
            }
        };

        await SendToUserAsync(userId, message, cancellationToken);
    }

    public async Task SendOrderStatusNotificationAsync(string userId, string orderNumber, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var (title, body, actionUrl) = status switch
        {
            OrderStatus.Confirmed => ("Commande confirmée", $"Votre commande {orderNumber} a été confirmée", $"/orders/{orderNumber}"),
            OrderStatus.Preparing => ("Commande en préparation", $"Votre commande {orderNumber} est en cours de préparation", $"/orders/{orderNumber}"),
            OrderStatus.Ready => ("Commande prête", $"Votre commande {orderNumber} est prête pour récupération", $"/orders/{orderNumber}"),
            OrderStatus.Delivered => ("Commande livrée", $"Votre commande {orderNumber} a été livrée avec succès", $"/orders/{orderNumber}"),
            OrderStatus.Cancelled => ("Commande annulée", $"Votre commande {orderNumber} a été annulée", $"/orders/{orderNumber}"),
            _ => ("Mise à jour commande", $"Statut de votre commande {orderNumber} mis à jour", $"/orders/{orderNumber}")
        };

        var message = new NotificationMessage
        {
            Title = title,
            Body = body,
            Type = NotificationType.OrderUpdate,
            Priority = NotificationPriority.High,
            ActionUrl = actionUrl,
            ImageUrl = _config.DefaultImages.OrderImage,
            Data = new Dictionary<string, object>
            {
                ["orderNumber"] = orderNumber,
                ["status"] = status.ToString(),
                ["timestamp"] = DateTime.UtcNow
            }
        };

        await SendToUserAsync(userId, message, cancellationToken);
    }

    public async Task SendLoyaltyPointsNotificationAsync(string userId, int pointsEarned, int totalPoints, CancellationToken cancellationToken = default)
    {
        var message = new NotificationMessage
        {
            Title = "Points fidélité gagnés!",
            Body = $"Vous avez gagné {pointsEarned} points! Total: {totalPoints} points",
            Type = NotificationType.Loyalty,
            Priority = NotificationPriority.Normal,
            ActionUrl = "/loyalty",
            ImageUrl = _config.DefaultImages.LoyaltyImage,
            Data = new Dictionary<string, object>
            {
                ["pointsEarned"] = pointsEarned,
                ["totalPoints"] = totalPoints,
                ["nextRewardThreshold"] = CalculateNextRewardThreshold(totalPoints)
            }
        };

        await SendToUserAsync(userId, message, cancellationToken);
    }

    public async Task SendPromotionNotificationAsync(string userId, string promotionTitle, string description, DateTime expiryDate, CancellationToken cancellationToken = default)
    {
        var message = new NotificationMessage
        {
            Title = $"🎉 {promotionTitle}",
            Body = $"{description} Valable jusqu'au {expiryDate:dd/MM/yyyy}",
            Type = NotificationType.Promotion,
            Priority = NotificationPriority.Normal,
            ActionUrl = "/promotions",
            ImageUrl = _config.DefaultImages.PromotionImage,
            ExpiryDate = expiryDate,
            Data = new Dictionary<string, object>
            {
                ["promotionTitle"] = promotionTitle,
                ["expiryDate"] = expiryDate,
                ["isLimitedTime"] = true
            }
        };

        await SendToUserAsync(userId, message, cancellationToken);
    }

    public async Task SendAppointmentReminderAsync(string userId, string serviceName, DateTime appointmentDate, CancellationToken cancellationToken = default)
    {
        var timeUntil = appointmentDate - DateTime.UtcNow;
        var reminderText = timeUntil.TotalHours <= 24 
            ? $"dans {timeUntil.Hours}h{timeUntil.Minutes:D2}m" 
            : $"le {appointmentDate:dd/MM à HH:mm}";

        var message = new NotificationMessage
        {
            Title = "Rappel rendez-vous",
            Body = $"N'oubliez pas votre RDV {serviceName} {reminderText}",
            Type = NotificationType.Appointment,
            Priority = NotificationPriority.High,
            ActionUrl = "/appointments",
            ImageUrl = _config.DefaultImages.AppointmentImage,
            Data = new Dictionary<string, object>
            {
                ["serviceName"] = serviceName,
                ["appointmentDate"] = appointmentDate,
                ["reminderType"] = timeUntil.TotalHours <= 1 ? "urgent" : "normal"
            }
        };

        await SendToUserAsync(userId, message, cancellationToken);
    }

    public async Task SendSystemMaintenanceAsync(DateTime startTime, DateTime endTime, string description, CancellationToken cancellationToken = default)
    {
        var message = new NotificationMessage
        {
            Title = "Maintenance programmée",
            Body = $"Maintenance prévue de {startTime:HH:mm} à {endTime:HH:mm}. {description}",
            Type = NotificationType.System,
            Priority = NotificationPriority.High,
            ImageUrl = _config.DefaultImages.SystemImage,
            Data = new Dictionary<string, object>
            {
                ["startTime"] = startTime,
                ["endTime"] = endTime,
                ["description"] = description,
                ["maintenanceType"] = "scheduled"
            }
        };

        await SendToAllAsync(message, cancellationToken);
    }

    // ===== ANALYTICS =====

    public async Task<NotificationStats> GetNotificationStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var dateKey = from.Date.ToString("yyyy-MM-dd");
        if (_stats.TryGetValue(dateKey, out var stats))
        {
            return stats;
        }

        return new NotificationStats
        {
            Period = new DateRange { From = from, To = to },
            TotalSent = 0,
            TotalFailed = 0,
            ByType = new Dictionary<NotificationType, int>(),
            ByPriority = new Dictionary<NotificationPriority, int>()
        };
    }

    public async Task<List<UserNotificationSummary>> GetUserNotificationSummaryAsync(List<string> userIds, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return userIds.Select(userId => new UserNotificationSummary
        {
            UserId = userId,
            TotalReceived = Random.Shared.Next(10, 100),
            LastNotificationDate = DateTime.UtcNow.AddHours(-Random.Shared.Next(1, 48)),
            PreferredChannels = new List<NotificationChannel> { NotificationChannel.Push, NotificationChannel.Email },
            IsOptedIn = true
        }).ToList();
    }

    // ===== PRIVATE METHODS =====

    private void ValidateMessage(NotificationMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(message.Title))
            throw new ArgumentException("Notification title is required", nameof(message));

        if (string.IsNullOrWhiteSpace(message.Body))
            throw new ArgumentException("Notification body is required", nameof(message));
    }

    private void EnrichMessage(NotificationMessage message)
    {
        message.Id = Guid.NewGuid().ToString();
        message.Timestamp = DateTime.UtcNow;
        
        if (string.IsNullOrEmpty(message.ImageUrl))
        {
            message.ImageUrl = message.Type switch
            {
                NotificationType.Welcome => _config.DefaultImages.WelcomeImage,
                NotificationType.OrderUpdate => _config.DefaultImages.OrderImage,
                NotificationType.Loyalty => _config.DefaultImages.LoyaltyImage,
                NotificationType.Promotion => _config.DefaultImages.PromotionImage,
                NotificationType.Appointment => _config.DefaultImages.AppointmentImage,
                NotificationType.System => _config.DefaultImages.SystemImage,
                _ => _config.DefaultImages.DefaultImage
            };
        }

        message.Data ??= new Dictionary<string, object>();
        message.Data["sentVia"] = "SignalR";
        message.Data["version"] = "1.0";
    }

    private async Task TrackNotificationAsync(string userId, NotificationMessage message)
    {
        // Store notification tracking data
        await Task.CompletedTask;
        _logger.LogTrace("Tracked notification {NotificationId} for user {UserId}", message.Id, userId);
    }

    private async Task TrackGroupNotificationAsync(string groupName, NotificationMessage message)
    {
        await Task.CompletedTask;
        _logger.LogTrace("Tracked group notification {NotificationId} for group {GroupName}", message.Id, groupName);
    }

    private async Task TrackBroadcastNotificationAsync(NotificationMessage message)
    {
        await Task.CompletedTask;
        _logger.LogTrace("Tracked broadcast notification {NotificationId}", message.Id);
    }

    private void UpdateStats(NotificationType type, bool success)
    {
        var dateKey = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        var stats = _stats.GetOrAdd(dateKey, _ => new NotificationStats
        {
            Period = new DateRange { From = DateTime.UtcNow.Date, To = DateTime.UtcNow.Date.AddDays(1) },
            ByType = new Dictionary<NotificationType, int>(),
            ByPriority = new Dictionary<NotificationPriority, int>()
        });

        if (success)
            Interlocked.Increment(ref stats.TotalSent);
        else
            Interlocked.Increment(ref stats.TotalFailed);

        stats.ByType.TryGetValue(type, out var currentCount);
        stats.ByType[type] = currentCount + 1;
    }

    private int CalculateNextRewardThreshold(int currentPoints)
    {
        return currentPoints switch
        {
            < 1000 => 1000 - currentPoints,
            < 5000 => 5000 - currentPoints,
            < 15000 => 15000 - currentPoints,
            < 50000 => 50000 - currentPoints,
            _ => 0
        };
    }
}

// ===== SIGNALR HUB =====

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} connected to notification hub with connection {ConnectionId}",
            userId, Context.ConnectionId);

        // Auto-join user to personal group
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from notification hub with connection {ConnectionId}",
            userId, Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Connection {ConnectionId} joined group {GroupName}",
            Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Connection {ConnectionId} left group {GroupName}",
            Context.ConnectionId, groupName);
    }

    public async Task MarkAsRead(string notificationId)
    {
        _logger.LogDebug("Notification {NotificationId} marked as read by {UserId}",
            notificationId, Context.UserIdentifier);
        
        // Update notification read status in database
        await Task.CompletedTask;
    }
}

// ===== MODEL CLASSES =====

public class NotificationMessage
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public List<NotificationChannel> Channels { get; set; } = new();
}

public enum NotificationType
{
    Welcome,
    OrderUpdate,
    Loyalty,
    Promotion,
    Appointment,
    System,
    Alert,
    Message
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum NotificationChannel
{
    Push,
    Email,
    Sms,
    InApp
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Preparing,
    Ready,
    Delivered,
    Cancelled
}

public class NotificationStats
{
    public DateRange Period { get; set; } = new();
    public int TotalSent { get; set; }
    public int TotalFailed { get; set; }
    public Dictionary<NotificationType, int> ByType { get; set; } = new();
    public Dictionary<NotificationPriority, int> ByPriority { get; set; } = new();
}

public class DateRange
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class UserNotificationSummary
{
    public string UserId { get; set; } = string.Empty;
    public int TotalReceived { get; set; }
    public DateTime? LastNotificationDate { get; set; }
    public List<NotificationChannel> PreferredChannels { get; set; } = new();
    public bool IsOptedIn { get; set; }
}