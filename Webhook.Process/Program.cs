using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Webhook.Process.Data;
using Webhook.Process.OpenTelemetry;
using Webhook.Process.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
// builder.Services.AddHttpClient();
builder.Services.AddDbContext<WebhooksDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks"));
});
builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.AddConsumer<WebhookDispatchedConsumer>();
    busConfig.AddConsumer<WebhookTriggeredConsumer>();

    busConfig.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        config.ConfigureEndpoints(context);
    });
});
builder.Services.AddOpenTelemetry()
    .WithTracing(trace =>
    {
        trace
            .AddSource(DiagnosticConfig.Source.Name)
            .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
            .AddNpgsql();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();