using System.Text.Json;
using GQL.GraphQL;
using Microsoft.EntityFrameworkCore;

namespace GQL.Data;

public class ViewManager
{
    private readonly AppDbContext _context;
    private readonly string _configPath;

    public ViewManager(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _configPath = System.IO.Path.Combine(env.ContentRootPath, "GraphQL", "productview-config.json");
    }

    public async Task RecreateProductsViewAsync()
    {
        // Read config file
        var configJson = await File.ReadAllTextAsync(_configPath);
        var config = JsonSerializer.Deserialize<ProductViewConfig>(configJson, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        }) ?? new ProductViewConfig();

        // Drop existing view
        await _context.Database.ExecuteSqlRawAsync("DROP VIEW IF EXISTS ProductsView;");

        // Build view SQL dynamically
        var selectFields = new List<string> { "Id" };
        
        foreach (var field in config.DynamicFields)
        {
            var sqlType = field.Type switch
            {
                "Int" => "INTEGER",
                "Decimal" or "Float" => "REAL",
                "Boolean" => "INTEGER",
                _ => "TEXT"
            };

            var jsonPathValue = field.JsonPath.Replace("$.", "");
            var castClause = field.Type == "Decimal" || field.Type == "Float" 
                ? $"CAST(json_extract(Data, '$.{jsonPathValue}') AS {sqlType})"
                : $"json_extract(Data, '$.{jsonPathValue}')";

            selectFields.Add($"{castClause} as {field.Name}");
        }

        selectFields.Add("Data");
        selectFields.Add("CreatedAt");
        selectFields.Add("UpdatedAt");

        var viewSql = $@"
            CREATE VIEW ProductsView AS
            SELECT 
                {string.Join(",\n                ", selectFields)}
            FROM Products;
        ";

        await _context.Database.ExecuteSqlRawAsync(viewSql);
        
        Console.WriteLine($"ProductsView recreated with {config.DynamicFields.Count} dynamic fields");
    }
}
