using System.Text.Json.Serialization;

namespace EFCore6.TemporalTables.API;

public class Product
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }
}
