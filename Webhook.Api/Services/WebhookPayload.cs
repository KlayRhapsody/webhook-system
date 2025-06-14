namespace Webhook.Api.Services;

internal sealed class WebhookPayload<T>
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public Guid SubscriptionId { get; set; }
    public DateTime Timestamp { get; set; }
    public T Data { get; set; }
}
