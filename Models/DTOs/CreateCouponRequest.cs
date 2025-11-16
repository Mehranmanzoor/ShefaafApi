using System;

namespace ShefaafAPI.Models.DTOs;

public class CreateCouponRequest
{
    public required string Code { get; set; }
    public required string DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime ExpiryDate { get; set; }
}
