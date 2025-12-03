using Microsoft.EntityFrameworkCore;
using GQL.Models;
using System.Text.Json;

namespace GQL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductView> ProductsView { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Data)
                .HasConversion(
                    v => v.HasValue ? v.Value.GetRawText() : null,
                    v => v != null ? JsonDocument.Parse(v, default).RootElement : (JsonElement?)null);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("datetime('now')");
        });

        // Configure ProductView entity (read-only view)
        modelBuilder.Entity<ProductView>(entity =>
        {
            entity.ToView("ProductsView");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Data)
                .HasConversion(
                    v => v.HasValue ? v.Value.GetRawText() : null,
                    v => v != null ? JsonDocument.Parse(v, default).RootElement : (JsonElement?)null);
        });

        // Seed data can be added here if needed
        // modelBuilder.Entity<Product>().HasData(...)
    }
}