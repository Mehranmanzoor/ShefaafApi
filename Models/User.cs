using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class User
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public required string Username { get; set; }
    
    public required string Email { get; set; }
    
    public required string Password { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? Address { get; set; }
    
    public string? City { get; set; }
    
    public string? PinCode { get; set; }
    
    public string Role { get; set; } = "Customer";
    
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}
