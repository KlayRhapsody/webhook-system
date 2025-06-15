using System.Diagnostics;

namespace Webhook.Api.OpenTelemetry;

internal static class DiagnosticConfig
{
    internal static readonly ActivitySource Source = new("webhooks-api");
}
