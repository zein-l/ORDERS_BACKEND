using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Data.Configurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);

        b.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        b.Property(x => x.FullName)
            .HasMaxLength(256);

        b.HasIndex(x => x.Email).IsUnique();

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc);
    }
}
