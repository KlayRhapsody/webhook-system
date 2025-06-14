namespace Webhook.Api.Models;

internal sealed class WebhookDeliverAttempt
{
    public Guid Id { get; set; }
    public Guid WebSubscriptionId { get; set; }
    public string Payload { get; set; }
    public int? ResponseStatusCode { get; set; }
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; }
}
