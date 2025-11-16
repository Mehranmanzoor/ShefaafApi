using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class Cart
{
    [JsonIgnore]
    public Guid CartId { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
