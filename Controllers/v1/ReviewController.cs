using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Models;
using ShefaafAPI.Models.DTOs;
using ShefaafAPI.Services;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly ISqlService _sqlService;

    public ReviewController(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpPost("Add")]
    [Authorize]
    public async Task<IActionResult> AddReview(AddReviewRequest model)
    {
        try
        {
            if (model.Rating < 1 || model.Rating > 5)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Rating must be between 1 and 5"
                });
            }

            var user = await _sqlService.FindUser(model.Email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var product = await _sqlService.GetProductById(model.ProductId);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found" });
            }

            var existingReview = await _sqlService.GetUserProductReview(user.UserId, model.ProductId);
            if (existingReview != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "You have already reviewed this product"
                });
            }

            var isVerifiedPurchase = await _sqlService.HasUserPurchasedProduct(user.UserId, model.ProductId);

            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                ProductId = model.ProductId,
                UserId = user.UserId,
                Username = user.Username,
                Rating = model.Rating,
                Comment = model.Comment,
                IsVerifiedPurchase = isVerifiedPurchase
            };

            await _sqlService.AddReview(review);

            return Ok(new
            {
                success = true,
                message = "Review added successfully",
                reviewId = review.ReviewId,
                isVerifiedPurchase
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("Product/{productId}")]
    public async Task<IActionResult> GetProductReviews(Guid productId)
    {
        try
        {
            var product = await _sqlService.GetProductById(productId);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found" });
            }

            var reviews = await _sqlService.GetProductReviews(productId);
            var averageRating = await _sqlService.GetProductAverageRating(productId);

            return Ok(new
            {
                success = true,
                productId,
                productName = product.Name,
                totalReviews = reviews.Count,
                averageRating = Math.Round(averageRating, 1),
                reviews
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpDelete("Delete/{reviewId}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid reviewId, [FromQuery] string email)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var review = await _sqlService.GetProductReviews(Guid.Empty);
            var userReview = review.FirstOrDefault(r => r.ReviewId == reviewId && r.UserId == user.UserId);

            if (userReview == null)
            {
                return NotFound(new { success = false, message = "Review not found or unauthorized" });
            }

            await _sqlService.DeleteReview(reviewId);

            return Ok(new { success = true, message = "Review deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }
}
