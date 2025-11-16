using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class Wishlist
{
    [JsonIgnore]
    public Guid WishlistId { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid ProductId { get; set; }
    
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
