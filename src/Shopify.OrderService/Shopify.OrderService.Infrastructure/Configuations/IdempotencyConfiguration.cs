using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopify.OrderService.Domain.Common.Idempotency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Infrastructure.Configuations;
public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");

        //the DB enforces uniqueness on the idempotent key column because it is a primary key - therefore handling the concurrency problem
        builder.HasKey(i => i.Key);
        builder.Property(i => i.Key).HasColumnName("key");

        builder.Property(i => i.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(i => i.ResponseBody)
            .HasColumnName("response_body")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // for performance when job deletes the old keys
        builder.HasIndex(i => i.CreatedAt)
            .HasDatabaseName("ix_idempotency_keys_created_at");
    }
}
