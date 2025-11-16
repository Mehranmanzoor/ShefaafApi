using System;

namespace ShefaafAPI.Models.DTOs;

public class ApplyCouponRequest
{
    public required string Email { get; set; }
    public required string CouponCode { get; set; }
    public decimal OrderAmount { get; set; }
}
