using Microsoft.EntityFrameworkCore;
using GQL.Data;
using GQL.Models;
using GQL.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Entity Framework
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Configure GraphQL
app.MapGraphQL();


// API Endpoints for Products
app.MapGet("/api/products", async (AppDbContext db) =>
{
    return await db.Products.ToListAsync();
})
.WithName("GetProducts")
.WithOpenApi();

app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProduct")
.WithOpenApi();

app.MapPost("/api/products", async (Product product, AppDbContext db) =>
{
    product.CreatedAt = DateTime.UtcNow;
    product.UpdatedAt = DateTime.UtcNow;
    
    db.Products.Add(product);
    await db.SaveChangesAsync();
    
    return Results.Created($"/api/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithOpenApi();

app.MapPut("/api/products/{id}", async (int id, Product inputProduct, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    
    product.Name = inputProduct.Name;
    product.Description = inputProduct.Description;
    product.Price = inputProduct.Price;
    product.Stock = inputProduct.Stock;
    product.UpdatedAt = DateTime.UtcNow;
    
    await db.SaveChangesAsync();
    
    return Results.NoContent();
})
.WithName("UpdateProduct")
.WithOpenApi();

app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    
    db.Products.Remove(product);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
})
.WithName("DeleteProduct")
.WithOpenApi();



app.Run();