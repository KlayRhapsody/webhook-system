namespace Webhook.Process.Services;

public sealed class WebhookPayload
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public Guid SubscriptionId { get; set; }
    public DateTime Timestamp { get; set; }
    public object Data { get; set; }
}
