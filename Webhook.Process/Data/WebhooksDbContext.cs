using Microsoft.EntityFrameworkCore;
using Webhook.Process.Models;

namespace Webhook.Process.Data;

internal sealed class WebhooksDbContext(DbContextOptions<WebhooksDbContext> options)
    : DbContext(options)
{
    public DbSet<WebhookSubscription> Subscriptions { get; set; }
    public DbSet<WebhookDeliverAttempt> WebhookDeliverAttempt { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
