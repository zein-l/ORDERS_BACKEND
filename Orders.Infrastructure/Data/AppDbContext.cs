using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>(); // <- NEW

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Pick up any IEntityTypeConfiguration<> in this assembly (safe to keep)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Order -> OrderItems
            // If OrderItem doesn't expose a back navigation (Order), use WithOne() without lambda.
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order -> User (FK on Order.UserId)
            modelBuilder.Entity<Order>()
                .HasOne<User>()
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AuditEvent mapping
            modelBuilder.Entity<AuditEvent>(e =>
            {
                e.ToTable("AuditEvents");
                e.HasKey(x => x.Id);
                e.Property(x => x.Action).HasMaxLength(128).IsRequired();
                e.Property(x => x.DetailsJson);
                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.OrderId);
                e.HasIndex(x => x.CreatedAtUtc);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
