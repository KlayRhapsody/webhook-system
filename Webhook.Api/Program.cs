using System.Threading.Channels;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Webhook.Api.Data;
using Webhook.Api.Extensions;
using Webhook.Api.Models;
using Webhook.Api.OpenTelemetry;
using Webhook.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddScoped<WebhookDispatcher>();
builder.Services.AddDbContext<WebhooksDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks"));
});
builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

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

WebApplication app = builder.Build();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });

    await app.ApplyMigrations();
}

app.UseHttpsRedirection();

// Create a subscription
app.MapPost("/webhooks/subscriptions", async (
    CreateWebhookRequest request,
    WebhooksDbContext dbContext) =>
{
    var subscription = new WebhookSubscription(
        Guid.NewGuid(),
        request.EventType,
        request.WebhookUrl,
        DateTime.UtcNow);

    dbContext.Subscriptions.Add(subscription);

    await dbContext.SaveChangesAsync();

    return Results.Ok(subscription);
})
.WithTags("WebhookSubscription");

// Create an order
app.MapPost("/orders", async (
    CreateOrderRequest request,
    WebhooksDbContext dbContext,
    WebhookDispatcher webhookDispatcher) =>
{
    var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

    dbContext.Orders.Add(order);

    await dbContext.SaveChangesAsync();

    await webhookDispatcher.DispatchAsync("order.created", order);

    return Results.Ok(order);
})
.WithTags("Orders");

app.MapGet("/orders", async (WebhooksDbContext dbContext) =>
{
    return Results.Ok(await dbContext.Orders.ToListAsync());
})
.WithTags("Orders");

app.Run();
