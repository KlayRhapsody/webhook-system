using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhook.Contracts;
using Webhook.Process.Data;
using Webhook.Process.Models;

namespace Webhook.Process.Services;

internal class WebhookDispatchedConsumer(WebhooksDbContext dbContext) : IConsumer<WebhookDispatched>
{
    public async Task Consume(ConsumeContext<WebhookDispatched> context)
    {
        WebhookDispatched message = context.Message;

        List<WebhookSubscription> subscriptions = await dbContext.Subscriptions
            .AsNoTracking()
            .Where(s => s.EventType == message.EventType)
            .ToListAsync();

        foreach (WebhookSubscription subscription in subscriptions)
        {
            await context.Publish(new WebhookTriggered(
                subscription.Id,
                subscription.EventType,
                subscription.WebhookUrl,
                message.Data));
        }

        // await context.PublishBatch(subscriptions.Select(s => new WebhookTriggered(
        //         s.Id,
        //         s.EventType,
        //         s.WebhookUrl,
        //         message.Data)));
    }
}
