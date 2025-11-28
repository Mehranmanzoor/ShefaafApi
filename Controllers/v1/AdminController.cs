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
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ISqlService sqlService, 
        IImageService imageService,
        ILogger<AdminController> logger)
    {
        _sqlService = sqlService;
        _imageService = imageService;
        _logger = logger;
    }

    [HttpGet("Users/All")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var allUsers = await _sqlService.GetAllUsers();
            var totalCount = allUsers.Count;
            
            var users = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    userId = u.UserId,
                    username = u.Username,
                    email = u.Email,
                    role = u.Role,
                    phoneNumber = u.PhoneNumber,
                    city = u.City,
                    isActive = u.IsActive,
                    createdAt = u.CreatedAt
                })
                .ToList();

            return Ok(new
            {
                success = true,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [HttpPut("Users/SetRole/{userId}")]
    public async Task<IActionResult> SetUserRole(Guid userId, [FromBody] string role)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new { success = false, message = "Role is required" });
            }

            role = role.Trim();

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

            _logger.LogInformation("User {UserId} role updated to {Role}", userId, role);

            return Ok(new
            {
                success = true,
                message = $"User role updated to {role}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for UserId: {UserId}", userId);
            return StatusCode(500, new { success = false, message = "Server Error" });
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

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
            {
                return BadRequest(new 
                { 
                    success = false, 
                    message = "Invalid file type. Only JPEG, PNG, and WebP images are allowed" 
                });
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (image.Length > maxFileSize)
            {
                return BadRequest(new 
                { 
                    success = false, 
                    message = "File too large. Maximum size is 5MB" 
                });
            }

            var imageUrl = await _imageService.UploadImage(image);

            _logger.LogInformation("Product image uploaded successfully: {ImageUrl}", imageUrl);

            return Ok(new
            {
                success = true,
                message = "Image uploaded successfully",
                imageUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading product image");
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [HttpGet("Dashboard/Stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var stats = await _sqlService.GetDashboardStatistics();

            return Ok(new
            {
                success = true,
                overview = new
                {
                    totalOrders = stats.TotalOrders,
                    totalRevenue = stats.TotalRevenue,
                    totalProducts = stats.TotalProducts,
                    totalUsers = stats.TotalUsers,
                    lowStockCount = stats.LowStockCount
                },
                orderStatus = new
                {
                    pending = stats.PendingOrders,
                    processing = stats.ProcessingOrders,
                    shipped = stats.ShippedOrders,
                    delivered = stats.DeliveredOrders,
                    cancelled = stats.CancelledOrders
                },
                today = new
                {
                    orders = stats.TodayOrders,
                    revenue = stats.TodayRevenue
                },
                thisMonth = new
                {
                    orders = stats.MonthlyOrders,
                    revenue = stats.MonthlyRevenue
                },
                lowStockProducts = stats.LowStockProducts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics");
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [HttpGet("Dashboard/RecentOrders")]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10)
    {
        try
        {
            if (limit < 1 || limit > 50)
            {
                limit = 10;
            }

            var allOrders = await _sqlService.GetAllOrders();
            var recentOrders = allOrders
                .OrderByDescending(o => o.CreatedAt)
                .Take(limit)
                .ToList();

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

            return Ok(new 
            { 
                success = true, 
                count = orderList.Count, 
                orders = orderList 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent orders with limit: {Limit}", limit);
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }

    [HttpGet("Products/LowStock")]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        try
        {
            if (threshold < 0 || threshold > 100)
            {
                threshold = 10;
            }

            var allProducts = await _sqlService.GetAllProducts();
            var lowStock = allProducts
                .Where(p => p.Stock <= threshold)
                .OrderBy(p => p.Stock)
                .ToList();

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
            _logger.LogError(ex, "Error fetching low stock products with threshold: {Threshold}", threshold);
            return StatusCode(500, new { success = false, message = "Server Error" });
        }
    }
}
