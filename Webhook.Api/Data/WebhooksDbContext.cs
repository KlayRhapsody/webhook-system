using Microsoft.EntityFrameworkCore;
using Webhook.Api.Models;

namespace Webhook.Api.Data;

internal sealed class WebhooksDbContext(DbContextOptions<WebhooksDbContext> options)
    : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<WebhookSubscription> Subscriptions { get; set; }
    public DbSet<WebhookDeliverAttempt> WebhookDeliverAttempt { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");

            builder.HasKey(b => b.Id);
        });

        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("subscriptions", "webhooks");

            builder.HasKey(b => b.Id);
        });

        modelBuilder.Entity<WebhookDeliverAttempt>(builder =>
        {
            builder.ToTable("delivery_attempts", "webhooks");

            builder.HasKey(b => b.Id);

            builder.HasOne<WebhookSubscription>()
                .WithMany()
                .HasForeignKey(b => b.WebSubscriptionId);
        });
    }
}
