using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopify.InventoryService.Infrastructure.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Infrastructure.Configurations;
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.Type).HasColumnName("type").IsRequired();
        builder.Property(m => m.Content).HasColumnName("content").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.ProcessedAt).HasColumnName("processed_at");
        builder.Property(m => m.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
        builder.Property(m => m.Error).HasColumnName("error");

        // Partial index — only unprocessed rows, keeps the publisher query fast
        builder.HasIndex(m => m.ProcessedAt)
            .HasDatabaseName("ix_outbox_messages_processed_at")
            .HasFilter("processed_at IS NULL");
    }
}
