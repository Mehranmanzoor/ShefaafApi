using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Models;
using ShefaafAPI.Models.DTOs;
using ShefaafAPI.Services;
using System.Security.Claims;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ISqlService _sqlService;

    public CartController(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddToCart(AddToCartRequest model)
    {
        try
        {
            var user = await _sqlService.FindUser(model.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            var product = await _sqlService.GetProductById(model.ProductId);
            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found"
                });
            }

            if (product.Stock < model.Quantity)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Insufficient stock",
                    availableStock = product.Stock
                });
            }

            var existingCart = await _sqlService.GetCartItem(user.UserId, model.ProductId);
            
            if (existingCart != null)
            {
                var newQuantity = existingCart.Quantity + model.Quantity;
                
                if (newQuantity > product.Stock)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Total quantity exceeds available stock",
                        currentInCart = existingCart.Quantity,
                        availableStock = product.Stock
                    });
                }

                await _sqlService.UpdateCartQuantity(existingCart.CartId, newQuantity);

                return Ok(new
                {
                    success = true,
                    message = "Cart updated successfully",
                    cartId = existingCart.CartId,
                    quantity = newQuantity
                });
            }
            else
            {
                var cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = user.UserId,
                    ProductId = model.ProductId,
                    Quantity = model.Quantity
                };

                await _sqlService.AddToCart(cart);

                return Ok(new
                {
                    success = true,
                    message = "Product added to cart successfully",
                    cartId = cart.CartId
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpGet("View")]
    public async Task<IActionResult> ViewCart([FromQuery] string email)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            var cartItems = await _sqlService.GetUserCart(user.UserId);
            
            var cartDetails = new List<object>();
            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var product = await _sqlService.GetProductById(item.ProductId);
                if (product != null)
                {
                    var itemTotal = product.Price * item.Quantity;
                    totalAmount += itemTotal;

                    cartDetails.Add(new
                    {
                        cartId = item.CartId,
                        productId = product.ProductId,
                        productName = product.Name,
                        price = product.Price,
                        quantity = item.Quantity,
                        total = itemTotal,
                        imageUrl = product.ImageUrl,
                        availableStock = product.Stock
                    });
                }
            }

            return Ok(new
            {
                success = true,
                email = user.Email,
                itemCount = cartItems.Count,
                items = cartDetails,
                totalAmount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpPut("Update")]
    public async Task<IActionResult> UpdateCart(UpdateCartRequest model)
    {
        try
        {
            var user = await _sqlService.FindUser(model.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            var cart = await _sqlService.GetUserCart(user.UserId);
            var cartItem = cart.FirstOrDefault(c => c.CartId == model.CartId);

            if (cartItem == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Cart item not found"
                });
            }

            var product = await _sqlService.GetProductById(cartItem.ProductId);
            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found"
                });
            }

            if (model.Quantity > product.Stock)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Quantity exceeds available stock",
                    availableStock = product.Stock
                });
            }

            await _sqlService.UpdateCartQuantity(model.CartId, model.Quantity);

            return Ok(new
            {
                success = true,
                message = "Cart updated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpDelete("Remove")]
    public async Task<IActionResult> RemoveFromCart([FromQuery] string email, [FromQuery] Guid cartId)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            var removed = await _sqlService.RemoveFromCart(cartId);
            
            if (!removed)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Cart item not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Item removed from cart"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpDelete("Clear")]
    public async Task<IActionResult> ClearCart([FromQuery] string email)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            await _sqlService.ClearCart(user.UserId);

            return Ok(new
            {
                success = true,
                message = "Cart cleared successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }
}
