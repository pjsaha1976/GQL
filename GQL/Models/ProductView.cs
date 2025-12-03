using System.Text.Json;

namespace GQL.Models;

public class ProductView
{
    public int Id { get; set; }
    public JsonElement? Data { get; set; }    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}