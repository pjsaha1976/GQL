using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using GQL.Data;
using GQL.Models;

namespace GQL.GraphQL;

public class Query
{
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProductView> GetProducts(AppDbContext context)
        => context.ProductsView.AsQueryable();

    public async Task<ProductView?> GetProductByIdAsync(
        int id,
        AppDbContext context,
        CancellationToken cancellationToken)
        => await context.ProductsView.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}