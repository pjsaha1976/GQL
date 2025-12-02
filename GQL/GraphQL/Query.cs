using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using GQL.Data;
using GQL.Models;

namespace GQL.GraphQL;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts(AppDbContext context)
        => context.Products;

    public async Task<Product?> GetProductByIdAsync(
        int id,
        AppDbContext context,
        CancellationToken cancellationToken)
        => await context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}