using HotChocolate;
using Microsoft.EntityFrameworkCore;
using GQL.Data;
using GQL.Models;
using System.Text.Json;

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
            Data = input.Data,
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

        product.Data = input.Data;
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
    JsonElement? Data
);