using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopify.OrderService.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Infrastructure.Configuations;
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(o => o.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<OrderStatus>(s))
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(o => o.ProductId)
            .HasDatabaseName("ix_orders_product_id");

        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("ix_orders_created_at");
    }
}