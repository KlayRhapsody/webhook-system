using Webhook.Api.Models;

namespace Webhook.Api.Repositories;

internal sealed class InMemoryWebhookSubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscription = [];

    public void Add(WebhookSubscription subscription)
    {
        _subscription.Add(subscription);
    }

    public IReadOnlyList<WebhookSubscription> GetByEventType(string eventType)
    {
        return _subscription.Where(s => s.EventType == eventType).ToList().AsReadOnly();
    }
}


