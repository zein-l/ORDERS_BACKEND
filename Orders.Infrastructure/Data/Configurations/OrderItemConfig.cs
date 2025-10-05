using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Data.Configurations;

public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");
        b.HasKey(x => x.Id);

        b.Property(x => x.OrderId).IsRequired();

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(x => x.Quantity)
            .IsRequired();

        b.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        // LineTotal is not stored; computed in code.
        b.Ignore(x => x.LineTotal);

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc);
    }
}
