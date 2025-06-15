using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Webhook.Api.Data;
using Webhook.Api.Models;
using Webhook.Api.OpenTelemetry;

namespace Webhook.Api.Services;

internal sealed class WebhookDispatcher(
    IHttpClientFactory httpClientFactory,
    WebhooksDbContext dbContext,
    Channel<WebhookDispatch> webhooksChannel)
{
    public async Task DispatchAsync(string eventType, object data)
    {
        using Activity? activity = DiagnosticConfig.Source.StartActivity($"{eventType} dispatch webhook");
        activity?.AddTag("event.type", eventType);
        await webhooksChannel.Writer.WriteAsync(new WebhookDispatch(eventType, data, activity?.Id));
    }

    public async Task ProcessAsync<T>(string eventType, T data)
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
