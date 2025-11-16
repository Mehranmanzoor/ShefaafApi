using System;
using System.Text.Json.Serialization;

namespace ShefaafAPI.Models;

public class Coupon
{
    [JsonIgnore]
    public Guid CouponId { get; set; }
    
    public required string Code { get; set; }
    
    public required string DiscountType { get; set; }
    
    public decimal DiscountValue { get; set; }
    
    public decimal? MinOrderAmount { get; set; }
    
    public decimal? MaxDiscountAmount { get; set; }
    
    public int? UsageLimit { get; set; }
    
    public int UsedCount { get; set; } = 0;
    
    public DateTime ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
