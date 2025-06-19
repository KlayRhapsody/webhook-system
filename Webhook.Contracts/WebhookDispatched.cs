namespace Webhook.Contracts;

public sealed record WebhookDispatched(string EventType, object Data);
