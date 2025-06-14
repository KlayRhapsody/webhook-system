using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Webhook.Api.Data;
using Webhook.Api.Models;

namespace Webhook.Api.Services;

internal sealed class WebhookDispatcher(
    IHttpClientFactory httpClientFactory,
    WebhooksDbContext dbContext)
{
    public async Task DispatchAsync<T>(string eventType, T data)
    {
        List<WebhookSubscription> subscriptions = await dbContext.Subscriptions
            .AsNoTracking()
            .Where(s => s.EventType == eventType)
            .ToListAsync();

        foreach (WebhookSubscription subscription in subscriptions)
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();

            var payload = new WebhookPayload<T>()
            {
                Id = Guid.NewGuid(),
                EventType = subscription.EventType,
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = data,
            };

            string jsonPayload = JsonSerializer.Serialize(payload);

            try
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                    subscription.WebhookUrl,
                    payload);

                var attempt = new WebhookDeliverAttempt()
                {
                    Id = Guid.NewGuid(),
                    WebSubscriptionId = subscription.Id,
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
                    WebSubscriptionId = subscription.Id,
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
}
