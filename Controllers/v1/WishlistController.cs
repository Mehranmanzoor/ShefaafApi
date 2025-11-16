using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Models;
using ShefaafAPI.Models.DTOs;
using ShefaafAPI.Services;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly ISqlService _sqlService;

    public WishlistController(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddToWishlist(AddToWishlistRequest model)
    {
        try
        {
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

            var existing = await _sqlService.GetWishlistItem(user.UserId, model.ProductId);
            if (existing != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Product already in wishlist"
                });
            }

            var wishlist = new Wishlist
            {
                WishlistId = Guid.NewGuid(),
                UserId = user.UserId,
                ProductId = model.ProductId
            };

            await _sqlService.AddToWishlist(wishlist);

            return Ok(new
            {
                success = true,
                message = "Product added to wishlist",
                wishlistId = wishlist.WishlistId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("View")]
    public async Task<IActionResult> ViewWishlist([FromQuery] string email)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var wishlistItems = await _sqlService.GetUserWishlist(user.UserId);
            
            var wishlistDetails = new List<object>();

            foreach (var item in wishlistItems)
            {
                var product = await _sqlService.GetProductById(item.ProductId);
                if (product != null)
                {
                    var avgRating = await _sqlService.GetProductAverageRating(product.ProductId);
                    
                    wishlistDetails.Add(new
                    {
                        wishlistId = item.WishlistId,
                        productId = product.ProductId,
                        productName = product.Name,
                        price = product.Price,
                        imageUrl = product.ImageUrl,
                        category = product.Category,
                        inStock = product.Stock > 0,
                        stock = product.Stock,
                        averageRating = Math.Round(avgRating, 1),
                        addedAt = item.CreatedAt
                    });
                }
            }

            return Ok(new
            {
                success = true,
                email = user.Email,
                itemCount = wishlistDetails.Count,
                items = wishlistDetails
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpDelete("Remove")]
    public async Task<IActionResult> RemoveFromWishlist([FromQuery] string email, [FromQuery] Guid wishlistId)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var removed = await _sqlService.RemoveFromWishlist(wishlistId);
            
            if (!removed)
            {
                return NotFound(new { success = false, message = "Wishlist item not found" });
            }

            return Ok(new { success = true, message = "Item removed from wishlist" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPost("MoveToCart")]
    public async Task<IActionResult> MoveToCart([FromQuery] string email, [FromQuery] Guid wishlistId, [FromQuery] int quantity = 1)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var wishlistItems = await _sqlService.GetUserWishlist(user.UserId);
            var wishlistItem = wishlistItems.FirstOrDefault(w => w.WishlistId == wishlistId);

            if (wishlistItem == null)
            {
                return NotFound(new { success = false, message = "Wishlist item not found" });
            }

            var product = await _sqlService.GetProductById(wishlistItem.ProductId);
            if (product == null || product.Stock < quantity)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Product out of stock or insufficient quantity"
                });
            }

            var existingCart = await _sqlService.GetCartItem(user.UserId, wishlistItem.ProductId);
            
            if (existingCart != null)
            {
                await _sqlService.UpdateCartQuantity(existingCart.CartId, existingCart.Quantity + quantity);
            }
            else
            {
                var cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = user.UserId,
                    ProductId = wishlistItem.ProductId,
                    Quantity = quantity
                };
                await _sqlService.AddToCart(cart);
            }

            await _sqlService.RemoveFromWishlist(wishlistId);

            return Ok(new
            {
                success = true,
                message = "Product moved to cart successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpDelete("Clear")]
    public async Task<IActionResult> ClearWishlist([FromQuery] string email)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            await _sqlService.ClearWishlist(user.UserId);

            return Ok(new { success = true, message = "Wishlist cleared successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }
}
