using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhook.Api.Data;
using Webhook.Api.Models;

namespace Webhook.Api.Services;

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
