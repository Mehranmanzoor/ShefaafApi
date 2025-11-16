using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Services;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly ISqlService _sqlService;
    private readonly IImageService _imageService;

    public AdminController(ISqlService sqlService, IImageService imageService)
    {
        _sqlService = sqlService;
        _imageService = imageService;
    }

    [HttpGet("Users/All")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _sqlService.GetAllUsers();
            
            var userList = users.Select(u => new
            {
                userId = u.UserId,
                username = u.Username,
                email = u.Email,
                role = u.Role,
                phoneNumber = u.PhoneNumber,
                city = u.City,
                isActive = u.IsActive,
                createdAt = u.CreatedAt
            }).ToList();

            return Ok(new
            {
                success = true,
                count = userList.Count,
                users = userList
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPut("Users/SetRole/{userId}")]
    public async Task<IActionResult> SetUserRole(Guid userId, [FromBody] string role)
    {
        try
        {
            if (role != "Admin" && role != "Customer")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid role. Must be 'Admin' or 'Customer'"
                });
            }

            var updated = await _sqlService.UpdateUserRole(userId, role);
            
            if (!updated)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new
            {
                success = true,
                message = $"User role updated to {role}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPost("Products/UploadImage")]
    public async Task<IActionResult> UploadProductImage([FromForm] IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(new { success = false, message = "No image file provided" });
            }

            var imageUrl = await _imageService.UploadImage(image);

            return Ok(new
            {
                success = true,
                message = "Image uploaded successfully",
                imageUrl
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("Dashboard/Stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var allOrders = await _sqlService.GetAllOrders();
            var allProducts = await _sqlService.GetAllProducts();
            var allUsers = await _sqlService.GetAllUsers();

            var totalOrders = allOrders.Count;
            var totalRevenue = allOrders.Sum(o => o.TotalAmount);
            var totalProducts = allProducts.Count;
            var totalUsers = allUsers.Count;
            
            var pendingOrders = allOrders.Count(o => o.Status == "Pending");
            var processingOrders = allOrders.Count(o => o.Status == "Processing");
            var shippedOrders = allOrders.Count(o => o.Status == "Shipped");
            var deliveredOrders = allOrders.Count(o => o.Status == "Delivered");
            var cancelledOrders = allOrders.Count(o => o.Status == "Cancelled");

            var today = DateTime.UtcNow.Date;
            var todayOrders = allOrders.Count(o => o.CreatedAt.Date == today);
            var todayRevenue = allOrders.Where(o => o.CreatedAt.Date == today).Sum(o => o.TotalAmount);

            var thisMonth = DateTime.UtcNow;
            var monthlyOrders = allOrders.Count(o => o.CreatedAt.Month == thisMonth.Month && o.CreatedAt.Year == thisMonth.Year);
            var monthlyRevenue = allOrders.Where(o => o.CreatedAt.Month == thisMonth.Month && o.CreatedAt.Year == thisMonth.Year).Sum(o => o.TotalAmount);

            var lowStockProducts = allProducts.Where(p => p.Stock < 10).Select(p => new
            {
                productId = p.ProductId,
                name = p.Name,
                stock = p.Stock,
                category = p.Category
            }).ToList();

            return Ok(new
            {
                success = true,
                overview = new
                {
                    totalOrders,
                    totalRevenue,
                    totalProducts,
                    totalUsers,
                    lowStockCount = lowStockProducts.Count
                },
                orderStatus = new
                {
                    pending = pendingOrders,
                    processing = processingOrders,
                    shipped = shippedOrders,
                    delivered = deliveredOrders,
                    cancelled = cancelledOrders
                },
                today = new
                {
                    orders = todayOrders,
                    revenue = todayRevenue
                },
                thisMonth = new
                {
                    orders = monthlyOrders,
                    revenue = monthlyRevenue
                },
                lowStockProducts
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("Dashboard/RecentOrders")]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10)
    {
        try
        {
            var allOrders = await _sqlService.GetAllOrders();
            var recentOrders = allOrders.Take(limit).ToList();

            var orderList = new List<object>();

            foreach (var order in recentOrders)
            {
                var items = await _sqlService.GetOrderItems(order.OrderId);
                
                orderList.Add(new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    paymentMethod = order.PaymentMethod,
                    city = order.City,
                    orderDate = order.CreatedAt,
                    itemCount = items.Count
                });
            }

            return Ok(new { success = true, count = orderList.Count, orders = orderList });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("Products/LowStock")]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        try
        {
            var allProducts = await _sqlService.GetAllProducts();
            var lowStock = allProducts.Where(p => p.Stock <= threshold).ToList();

            return Ok(new
            {
                success = true,
                threshold,
                count = lowStock.Count,
                products = lowStock
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }
}
