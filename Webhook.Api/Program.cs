using Webhook.Api.Models;
using Webhook.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<InMemoryOrderRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });
}

app.UseHttpsRedirection();

// Create an order
app.MapPost("/orders", (
    CreateOrderRequest request,
    InMemoryOrderRepository orderRepository) =>
{
    var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

    orderRepository.Add(order);

    return Results.Ok(order);
})
.WithTags("Orders");

app.MapGet("/orders", (InMemoryOrderRepository orderRepository) =>
{
    return Results.Ok(orderRepository.GetAll());
})
.WithTags("Orders");

app.Run();
