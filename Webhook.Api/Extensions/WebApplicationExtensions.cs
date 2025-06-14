using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Webhook.Api.Data;

namespace Webhook.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        WebhooksDbContext db = scope.ServiceProvider.GetRequiredService<WebhooksDbContext>();

        await db.Database.MigrateAsync();
    }
}
