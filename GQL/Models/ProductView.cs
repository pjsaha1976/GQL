using System.Text.Json;

namespace GQL.Models;

public class ProductView
{
    public int Id { get; set; }
    public JsonElement? Data { get; set; }

}