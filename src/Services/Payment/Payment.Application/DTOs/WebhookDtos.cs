// DTOs pour les webhooks dans le namespace global
public class StripeWebhookEvent
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public StripeEventData? Data { get; set; }
}

public class StripeEventData
{
    public StripeObject? Object { get; set; }
}

public class StripeObject
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
}

public class PayPalWebhookEvent
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public PayPalResource? Resource { get; set; }
}

public class PayPalResource
{
    public string Id { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}