using System.Text.Json.Serialization;

namespace GQL.GraphQL;

public class ProductViewConfig
{
    [JsonPropertyName("dynamicFields")]
    public List<DynamicField> DynamicFields { get; set; } = new();
}

public class DynamicField
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("jsonPath")]
    public string JsonPath { get; set; } = string.Empty;
}
