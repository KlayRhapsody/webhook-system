using Webhook.Api.Models;
using Webhook.Api.Repositories;
using Webhook.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<InMemoryOrderRepository>();
builder.Services.AddSingleton<InMemoryWebhookSubscriptionRepository>();
builder.Services.AddHttpClient<WebhookDispatcher>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });
}

app.UseHttpsRedirection();

// Create a subscription
app.MapPost("/webhooks/subscriptions", (
    CreateWebhookRequest request,
    InMemoryWebhookSubscriptionRepository subscriptionRepository) =>
{
    var subscription = new WebhookSubscription(
        Guid.NewGuid(),
        request.EventType,
        request.WebhookUrl,
        DateTime.UtcNow);

    subscriptionRepository.Add(subscription);

    return Results.Ok(subscription);
})
.WithTags("WebhookSubscription");

// Create an order
app.MapPost("/orders", async (
    CreateOrderRequest request,
    InMemoryOrderRepository orderRepository,
    WebhookDispatcher webhookDispatcher) =>
{
    var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

    orderRepository.Add(order);

    await webhookDispatcher.DispatchAsync("order.created", order);

    return Results.Ok(order);
})
.WithTags("Orders");

app.MapGet("/orders", (InMemoryOrderRepository orderRepository) =>
{
    return Results.Ok(orderRepository.GetAll());
})
.WithTags("Orders");

app.Run();
