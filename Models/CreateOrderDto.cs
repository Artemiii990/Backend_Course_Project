using System.Text.Json.Serialization;

public class CreateOrderDto
{
    [JsonPropertyName("productName")]
    public string ProductName { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}