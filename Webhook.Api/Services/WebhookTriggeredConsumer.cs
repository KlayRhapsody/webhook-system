using System.Text.Json;
using MassTransit;
using Webhook.Api.Data;
using Webhook.Api.Models;

namespace Webhook.Api.Services;

internal sealed class WebhookTriggeredConsumer(
    IHttpClientFactory httpClientFactory,
    WebhooksDbContext dbContext) : IConsumer<WebhookTriggered>
{
    public async Task Consume(ConsumeContext<WebhookTriggered> context)
    {
        using HttpClient httpClient = httpClientFactory.CreateClient();

        var payload = new WebhookPayload()
        {
            Id = Guid.NewGuid(),
            EventType = context.Message.EventType,
            SubscriptionId = context.Message.SubscriptionId,
            Timestamp = DateTime.UtcNow,
            Data = context.Message.Data,
        };

        string jsonPayload = JsonSerializer.Serialize(payload);

        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                context.Message.WebhookUrl,
                payload);
            
            response.EnsureSuccessStatusCode();

            var attempt = new WebhookDeliverAttempt()
            {
                Id = Guid.NewGuid(),
                WebSubscriptionId = context.Message.SubscriptionId,
                Payload = jsonPayload,
                ResponseStatusCode = (int)response.StatusCode,
                Success = response.IsSuccessStatusCode,
                Timestamp = DateTime.UtcNow
            };

            dbContext.WebhookDeliverAttempt.Add(attempt);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            var attempt = new WebhookDeliverAttempt()
            {
                Id = Guid.NewGuid(),
                WebSubscriptionId = context.Message.SubscriptionId,
                Payload = jsonPayload,
                ResponseStatusCode = null,
                Success = false,
                Timestamp = DateTime.UtcNow
            };

            dbContext.WebhookDeliverAttempt.Add(attempt);

            await dbContext.SaveChangesAsync();
        }
    }
}
