using Microsoft.EntityFrameworkCore;
using GQL.Data;
using GQL.Models;
using GQL.GraphQL;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Entity Framework
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Load ProductView configuration
var configPath = System.IO.Path.Combine(builder.Environment.ContentRootPath, "GraphQL", "productview-config.json");
Console.WriteLine($"Loading config from: {configPath}");
var configJson = System.IO.File.ReadAllText(configPath);
Console.WriteLine($"Config JSON: {configJson}");
var productViewConfig = JsonSerializer.Deserialize<ProductViewConfig>(configJson, new JsonSerializerOptions 
{ 
    PropertyNameCaseInsensitive = true 
}) ?? new ProductViewConfig();
Console.WriteLine($"Loaded {productViewConfig.DynamicFields.Count} dynamic fields");
builder.Services.AddSingleton(productViewConfig);
builder.Services.AddSingleton<ProductViewTypeModule>();
builder.Services.AddScoped<ViewManager>();

// Add GraphQL with hot reload support
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeModule<ProductViewTypeModule>()
    .AddFiltering()
    .AddProjections()
    .AddSorting()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

// Recreate ProductsView based on config
using (var scope = app.Services.CreateScope())
{
    var viewManager = scope.ServiceProvider.GetRequiredService<ViewManager>();
    await viewManager.RecreateProductsViewAsync();
}

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
    return await db.ProductsView.ToListAsync();
})
.WithName("GetProducts")
.WithOpenApi();

app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.ProductsView.FirstOrDefaultAsync(p => p.Id == id);
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
    
    product.Data = inputProduct.Data;
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