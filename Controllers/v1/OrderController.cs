using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Models;
using ShefaafAPI.Models.DTOs;
using ShefaafAPI.Services;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly ISqlService _sqlService;

    public OrderController(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpPost("Place")]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest model)
    {
        try
        {
            var user = await _sqlService.FindUser(model.Email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var cartItems = await _sqlService.GetUserCart(user.UserId);
            if (cartItems.Count == 0)
            {
                return BadRequest(new { success = false, message = "Cart is empty" });
            }

            decimal totalAmount = 0;
            var orderItems = new List<object>();

            foreach (var item in cartItems)
            {
                var product = await _sqlService.GetProductById(item.ProductId);
                if (product == null) continue;

                if (product.Stock < item.Quantity)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"{product.Name} is out of stock or insufficient quantity",
                        availableStock = product.Stock
                    });
                }

                var itemTotal = product.Price * item.Quantity;
                totalAmount += itemTotal;

                orderItems.Add(new
                {
                    productId = product.ProductId,
                    productName = product.Name,
                    price = product.Price,
                    quantity = item.Quantity,
                    total = itemTotal
                });
            }

            var orderNumber = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}";
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = user.UserId,
                TotalAmount = totalAmount,
                Status = "Pending",
                ShippingAddress = model.ShippingAddress,
                City = model.City,
                PinCode = model.PinCode,
                PhoneNumber = model.PhoneNumber,
                PaymentMethod = model.PaymentMethod ?? "COD",
                PaymentStatus = "Pending"
            };

            await _sqlService.CreateOrder(order);

            foreach (var item in cartItems)
            {
                var product = await _sqlService.GetProductById(item.ProductId);
                if (product == null) continue;

                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = item.Quantity,
                    Total = product.Price * item.Quantity
                };

                await _sqlService.CreateOrderItem(orderItem);
                await _sqlService.UpdateProductStock(product.ProductId, product.Stock - item.Quantity);
            }

            await _sqlService.ClearCart(user.UserId);

            return Ok(new
            {
                success = true,
                message = "Order placed successfully",
                orderId = order.OrderId,
                orderNumber = order.OrderNumber,
                totalAmount = order.TotalAmount,
                items = orderItems
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPost("Cancel/{orderId}")]
    public async Task<IActionResult> CancelOrder(Guid orderId, CancelOrderRequest model)
    {
        try
        {
            var user = await _sqlService.FindUser(model.Email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var order = await _sqlService.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Order not found" });
            }

            if (order.UserId != user.UserId)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "You are not authorized to cancel this order"
                });
            }

            if (order.IsCancelled)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Order is already cancelled"
                });
            }

            if (order.Status == "Delivered")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot cancel delivered order"
                });
            }

            if (order.Status == "Shipped")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Order has already been shipped. Please contact customer support."
                });
            }

            var cancelled = await _sqlService.CancelOrder(orderId, model.CancellationReason);

            if (!cancelled)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to cancel order"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Order cancelled successfully",
                orderId,
                refundStatus = order.PaymentMethod == "COD" ? "No refund required" : "Refund will be processed in 5-7 business days"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("MyOrders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] string email)
    {
        try
        {
            var user = await _sqlService.FindUser(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var orders = await _sqlService.GetUserOrders(user.UserId);
            var orderList = new List<object>();

            foreach (var order in orders)
            {
                var items = await _sqlService.GetOrderItems(order.OrderId);
                orderList.Add(new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    isCancelled = order.IsCancelled,
                    cancellationReason = order.CancellationReason,
                    cancelledAt = order.CancelledAt,
                    paymentMethod = order.PaymentMethod,
                    paymentStatus = order.PaymentStatus,
                    shippingAddress = order.ShippingAddress,
                    city = order.City,
                    orderDate = order.CreatedAt,
                    itemCount = items.Count
                });
            }

            return Ok(new { success = true, email = user.Email, orderCount = orders.Count, orders = orderList });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("Details/{orderId}")]
    public async Task<IActionResult> GetOrderDetails(Guid orderId)
    {
        try
        {
            var order = await _sqlService.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Order not found" });
            }

            var items = await _sqlService.GetOrderItems(orderId);

            return Ok(new
            {
                success = true,
                order = new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    isCancelled = order.IsCancelled,
                    cancellationReason = order.CancellationReason,
                    cancelledAt = order.CancelledAt,
                    paymentMethod = order.PaymentMethod,
                    paymentStatus = order.PaymentStatus,
                    shippingAddress = order.ShippingAddress,
                    city = order.City,
                    pinCode = order.PinCode,
                    phoneNumber = order.PhoneNumber,
                    orderDate = order.CreatedAt,
                    items
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpPut("UpdateStatus/{orderId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, UpdateOrderStatusRequest model)
    {
        try
        {
            var order = await _sqlService.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Order not found" });
            }

            if (order.IsCancelled)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot update status of cancelled order"
                });
            }

            var updated = await _sqlService.UpdateOrderStatus(orderId, model.Status);
            
            if (!updated)
            {
                return NotFound(new { success = false, message = "Order not found" });
            }

            return Ok(new { success = true, message = $"Order status updated to {model.Status}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }

    [HttpGet("All")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var orders = await _sqlService.GetAllOrders();
            var orderList = new List<object>();

            foreach (var order in orders)
            {
                var items = await _sqlService.GetOrderItems(order.OrderId);
                
                orderList.Add(new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    userId = order.UserId,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    isCancelled = order.IsCancelled,
                    paymentMethod = order.PaymentMethod,
                    paymentStatus = order.PaymentStatus,
                    city = order.City,
                    orderDate = order.CreatedAt,
                    itemCount = items.Count
                });
            }

            return Ok(new { success = true, orderCount = orders.Count, orders = orderList });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server Error", error = ex.Message });
        }
    }
}
