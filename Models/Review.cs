using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class Review
{
    [JsonIgnore]
    public Guid ReviewId { get; set; }
    
    public Guid ProductId { get; set; }
    
    public Guid UserId { get; set; }
    
    public required string Username { get; set; }
    
    public int Rating { get; set; }
    
    public string? Comment { get; set; }
    
    public bool IsVerifiedPurchase { get; set; } = false;
    
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
