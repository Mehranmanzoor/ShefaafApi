using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class OrderItem
{
    [JsonIgnore]
    public Guid OrderItemId { get; set; }
    
    public Guid OrderId { get; set; }
    
    public Guid ProductId { get; set; }
    
    public required string ProductName { get; set; }
    
    public decimal Price { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal Total { get; set; }
}
