using System.Diagnostics;
using MassTransit;
using Webhook.Api.OpenTelemetry;
using Webhook.Contracts;

namespace Webhook.Api.Services;

internal sealed class WebhookDispatcher(
    IPublishEndpoint publishEndpoint)
{
    public async Task DispatchAsync(string eventType, object data)
    {
        using Activity? activity = DiagnosticConfig.Source.StartActivity($"{eventType} dispatch webhook");
        activity?.AddTag("event.type", eventType);
        await publishEndpoint.Publish(new WebhookDispatched(eventType, data));
    }
}
