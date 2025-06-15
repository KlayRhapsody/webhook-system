using System.Diagnostics;
using System.Threading.Channels;
using Webhook.Api.OpenTelemetry;

namespace Webhook.Api.Services;

internal sealed class WebhookProcessor(
    IServiceScopeFactory serviceScopeFactory,
    Channel<WebhookDispatch> webhooksChannel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (WebhookDispatch dispatch in webhooksChannel.Reader.ReadAllAsync())
        {
            using Activity? activity = DiagnosticConfig.Source.StartActivity(
                $"{dispatch.EventType} process webhook",
                ActivityKind.Internal,
                parentId: dispatch.ParentActivityId);

            using IServiceScope scope = serviceScopeFactory.CreateScope();

            WebhookDispatcher webhookDispatcher = scope.ServiceProvider
                .GetRequiredService<WebhookDispatcher>();

            await webhookDispatcher.ProcessAsync(dispatch.EventType, dispatch.Data);
        }
    }
}
