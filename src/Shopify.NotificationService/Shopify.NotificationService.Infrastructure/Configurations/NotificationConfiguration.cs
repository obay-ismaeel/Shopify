using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopify.NotificationService.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Infrastructure.Configurations;
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");

        builder.Property(n => n.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasConversion(
                t => t.ToString(),
                t => Enum.Parse<NotificationType>(t))
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<NotificationStatus>(s))
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0);

        builder.Property(n => n.FailureReason)
            .HasColumnName("failure_reason");

        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Composite unique index — core of the idempotency guarantee.
        // The database itself prevents duplicate notifications for the same order + type,
        // even under concurrent requests.
        builder.HasIndex(n => new { n.OrderId, n.Type })
            .IsUnique()
            .HasDatabaseName("ix_notifications_order_id_type");

        // Used by RetryFailedNotificationsService to efficiently query failed records
        builder.HasIndex(n => n.Status)
            .HasDatabaseName("ix_notifications_status");
    }
}