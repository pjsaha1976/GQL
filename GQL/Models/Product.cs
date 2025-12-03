using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace GQL.Models;

public class Product
{
    public int Id { get; set; }
    public JsonElement? Data { get; set; }    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}