using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace GQL.Models;

public class Product
{
    public int Id { get; set; }
    public JsonElement? Data { get; set; }
    
    [NotMapped]
    public string? Name => Data.HasValue && Data.Value.TryGetProperty("name", out var name) ? name.GetString() : null;
    
    [NotMapped]
    public string? Description => Data.HasValue && Data.Value.TryGetProperty("description", out var desc) ? desc.GetString() : null;
    
    [NotMapped]
    public decimal? Price => Data.HasValue && Data.Value.TryGetProperty("price", out var price) && price.ValueKind == JsonValueKind.Number ? price.GetDecimal() : null;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}