using Microsoft.EntityFrameworkCore;
using GQL.Models;

namespace GQL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("datetime('now')");
        });

        // Seed data can be added here if needed
        // modelBuilder.Entity<Product>().HasData(...)
    }
}