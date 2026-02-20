using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopify.InventoryService.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Infrastructure.Configurations;
public class ProcessedOrderConfiguration : IEntityTypeConfiguration<ProcessedOrder>
{
    public void Configure(EntityTypeBuilder<ProcessedOrder> builder)
    {
        builder.ToTable("processed_orders");

        builder.HasKey(p => p.OrderId);
        builder.Property(p => p.OrderId).HasColumnName("order_id");

        builder.Property(p => p.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(p => p.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();
    }
}
