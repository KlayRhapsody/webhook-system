using System.Diagnostics;

namespace Webhook.Process.OpenTelemetry;

internal static class DiagnosticConfig
{
    internal static readonly ActivitySource Source = new("webhooks-process");
}
