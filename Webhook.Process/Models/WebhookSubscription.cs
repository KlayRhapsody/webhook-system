namespace Webhook.Process.Models;

public sealed record WebhookSubscription(Guid Id, string EventType, string WebhookUrl, DateTime CreateOnUtc);

public sealed record CreateWebhookRequest(string EventType, string WebhookUrl);
