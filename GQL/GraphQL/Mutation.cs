using HotChocolate;
using Microsoft.EntityFrameworkCore;
using GQL.Data;
using GQL.Models;

namespace GQL.GraphQL;

public class Mutation
{
    public async Task<Product> CreateProductAsync(
        ProductInput input,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = input.Name,
            Description = input.Description,
            Price = input.Price,
            Stock = input.Stock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<Product> UpdateProductAsync(
        int id,
        ProductInput input,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null)
        {
            throw new GraphQLException("Product not found");
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Price = input.Price;
        product.Stock = input.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<bool> DeleteProductAsync(
        int id,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null)
        {
            return false;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public record ProductInput(
    string Name,
    string? Description,
    decimal Price,
    int Stock
);