using BunningsSizzlingHotProducts.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BunningsSizzlingHotProducts.Infrastructure.Persistence;

public sealed class SizzlingHotProductsDbContext(DbContextOptions<SizzlingHotProductsDbContext> options)
    : DbContext(options)
{
    public DbSet<ProductRow> Products => Set<ProductRow>();
    public DbSet<OrderRow> Orders => Set<OrderRow>();
    public DbSet<OrderEntryRow> OrderEntries => Set<OrderEntryRow>();

    // Tables Design
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ProductRow>(e =>
        {
            e.ToTable("products");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasMaxLength(32);
            e.Property(p => p.Name).HasMaxLength(256);
        });

        b.Entity<OrderRow>(e =>
        {
            e.ToTable("orders");
            e.HasKey(o => o.OrderId);
            e.Property(o => o.OrderId).HasMaxLength(64);
            e.Property(o => o.Status).HasMaxLength(16);
            e.HasIndex(o => o.Date);
            e.HasIndex(o => o.OriginalOrderDate);
            e.HasMany(o => o.Entries)
             .WithOne()
             .HasForeignKey(en => en.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<OrderEntryRow>(e =>
        {
            e.ToTable("order_entries");
            e.HasKey(en => en.Id);
            e.Property(en => en.Id).ValueGeneratedOnAdd();
        });
    }
}
