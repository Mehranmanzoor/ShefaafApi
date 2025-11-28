using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class Order
{
    [JsonIgnore]
    public Guid OrderId { get; set; }
    
    public required string OrderNumber { get; set; }
    
    public Guid UserId { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public required string Status { get; set; } = "Pending";
    
    public required string ShippingAddress { get; set; }
    
    public required string City { get; set; }
    
    public required string PinCode { get; set; }
    
    public required string PhoneNumber { get; set; }
    
    public string? PaymentMethod { get; set; } = "COD";
    
    public string? PaymentStatus { get; set; } = "Pending";
    
    public bool IsCancelled { get; set; } = false;
    
    public string? CancellationReason { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;



}
