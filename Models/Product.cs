using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class Product
{
    [JsonIgnore]
    public Guid ProductId { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public decimal Price { get; set; }
    
    public int Stock { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public string? Category { get; set; }
    
    public string? Weight { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
