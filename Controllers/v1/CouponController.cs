using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Models;
using ShefaafAPI.Models.DTOs;
using ShefaafAPI.Services;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
public class CouponController : ControllerBase
{
    private readonly ISqlService _sqlService;

    public CouponController(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpPost("Create")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateCoupon(CreateCouponRequest model)
    {
        try
        {
            if (model.DiscountType != "Percentage" && model.DiscountType != "Fixed")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "DiscountType must be 'Percentage' or 'Fixed'"
                });
            }

            if (model.DiscountType == "Percentage" && (model.DiscountValue < 0 || model.DiscountValue > 100))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Percentage discount must be between 0 and 100"
                });
            }

            var existingCoupon = await _sqlService.GetCouponByCode(model.Code);
            if (existingCoupon != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Coupon code already exists"
                });
            }

            var coupon = new Coupon
            {
                CouponId = Guid.NewGuid(),
                Code = model.Code.ToUpper(),
                DiscountType = model.DiscountType,
                DiscountValue = model.DiscountValue,
                MinOrderAmount = model.MinOrderAmount,
                MaxDiscountAmount = model.MaxDiscountAmount,
                UsageLimit = model.UsageLimit,
                ExpiryDate = model.ExpiryDate
            };

            await _sqlService.CreateCoupon(coupon);

            return Ok(new
            {
                success = true,
                message = "Coupon created successfully",
                coupon
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPost("Apply")]
    [Authorize]
    public async Task<IActionResult> ApplyCoupon(ApplyCouponRequest model)
    {
        try
        {
            var user = await _sqlService.FindUser(model.Email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var coupon = await _sqlService.GetCouponByCode(model.CouponCode);
            
            if (coupon == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Invalid coupon code"
                });
            }

            if (!coupon.IsActive)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Coupon is no longer active"
                });
            }

            if (coupon.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Coupon has expired"
                });
            }

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Coupon usage limit reached"
                });
            }

            if (coupon.MinOrderAmount.HasValue && model.OrderAmount < coupon.MinOrderAmount.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Minimum order amount of {coupon.MinOrderAmount.Value} required"
                });
            }

            decimal discountAmount = 0;

            if (coupon.DiscountType == "Percentage")
            {
                discountAmount = (model.OrderAmount * coupon.DiscountValue) / 100;
                
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }
            }
            else
            {
                discountAmount = coupon.DiscountValue;
            }

            var finalAmount = model.OrderAmount - discountAmount;

            return Ok(new
            {
                success = true,
                message = "Coupon applied successfully",
                couponCode = coupon.Code,
                discountType = coupon.DiscountType,
                discountValue = coupon.DiscountValue,
                orderAmount = model.OrderAmount,
                discountAmount = Math.Round(discountAmount, 2),
                finalAmount = Math.Round(finalAmount, 2),
                saved = Math.Round(discountAmount, 2)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("All")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllCoupons()
    {
        try
        {
            var coupons = await _sqlService.GetAllCoupons();

            var couponList = coupons.Select(c => new
            {
                couponId = c.CouponId,
                code = c.Code,
                discountType = c.DiscountType,
                discountValue = c.DiscountValue,
                minOrderAmount = c.MinOrderAmount,
                maxDiscountAmount = c.MaxDiscountAmount,
                usageLimit = c.UsageLimit,
                usedCount = c.UsedCount,
                remainingUses = c.UsageLimit.HasValue ? (int?)(c.UsageLimit.Value - c.UsedCount) : null,
                expiryDate = c.ExpiryDate,
                isActive = c.IsActive,
                isExpired = c.ExpiryDate < DateTime.UtcNow,
                createdAt = c.CreatedAt
            }).ToList();

            return Ok(new
            {
                success = true,
                count = couponList.Count,
                coupons = couponList
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPut("Deactivate/{couponId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeactivateCoupon(Guid couponId)
    {
        try
        {
            var deactivated = await _sqlService.DeactivateCoupon(couponId);
            
            if (!deactivated)
            {
                return NotFound(new { success = false, message = "Coupon not found" });
            }

            return Ok(new { success = true, message = "Coupon deactivated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }
}
