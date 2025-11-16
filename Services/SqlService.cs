using Microsoft.EntityFrameworkCore;
using ShefaafAPI.Data;
using ShefaafAPI.Models;

namespace ShefaafAPI.Services;

public class SqlService : ISqlService
{
    private readonly AppDbContext db;

    public SqlService(AppDbContext db)
    {
        this.db = db;
    }

    // User methods
    public async Task<User?> FindUser(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> FindUserById(Guid userId)
    {
        return await db.Users.FindAsync(userId);
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await db.Users.ToListAsync();
    }

    public async Task CreateUser(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteUser(string email)
    {
        var user = await FindUser(email);
        if (user == null) return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePass(Guid userId, string newPassword)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return false;

        user.Password = newPassword;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserRole(Guid userId, string role)
    {
        var user = await FindUserById(userId);
        if (user == null) return false;

        user.Role = role;
        await db.SaveChangesAsync();
        return true;
    }

    // Product methods
    public async Task<List<Product>> GetAllProducts()
    {
        return await db.Products.Where(p => p.IsActive).ToListAsync();
    }

    public async Task<Product?> GetProductById(Guid productId)
    {
        return await db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<List<Product>> GetProductsByCategory(string category)
    {
        return await db.Products
            .Where(p => p.IsActive && p.Category == category)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchProducts(string searchTerm)
    {
        return await db.Products
            .Where(p => p.IsActive && 
                   (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task CreateProduct(Product product)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateProduct(Product product)
    {
        var existing = await GetProductById(product.ProductId);
        if (existing == null) return false;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.ImageUrl = product.ImageUrl;
        existing.Category = product.Category;
        existing.Weight = product.Weight;
        existing.IsActive = product.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProduct(Guid productId)
    {
        var product = await GetProductById(productId);
        if (product == null) return false;

        product.IsActive = false;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProductStock(Guid productId, int stock)
    {
        var product = await GetProductById(productId);
        if (product == null) return false;

        product.Stock = stock;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    // Cart methods
    public async Task<List<Cart>> GetUserCart(Guid userId)
    {
        return await db.Carts
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<Cart?> GetCartItem(Guid userId, Guid productId)
    {
        return await db.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
    }

    public async Task AddToCart(Cart cart)
    {
        db.Carts.Add(cart);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateCartQuantity(Guid cartId, int quantity)
    {
        var cart = await db.Carts.FindAsync(cartId);
        if (cart == null) return false;

        cart.Quantity = quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromCart(Guid cartId)
    {
        var cart = await db.Carts.FindAsync(cartId);
        if (cart == null) return false;

        db.Carts.Remove(cart);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCart(Guid userId)
    {
        var cartItems = await GetUserCart(userId);
        db.Carts.RemoveRange(cartItems);
        await db.SaveChangesAsync();
        return true;
    }

    // Order methods
    public async Task CreateOrder(Order order)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }

    public async Task CreateOrderItem(OrderItem orderItem)
    {
        db.OrderItems.Add(orderItem);
        await db.SaveChangesAsync();
    }

    public async Task<List<Order>> GetUserOrders(Guid userId)
    {
        return await db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderById(Guid orderId)
    {
        return await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<OrderItem>> GetOrderItems(Guid orderId)
    {
        return await db.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<bool> UpdateOrderStatus(Guid orderId, string status)
    {
        var order = await GetOrderById(orderId);
        if (order == null) return false;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Order>> GetAllOrders()
    {
        return await db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasUserPurchasedProduct(Guid userId, Guid productId)
    {
        var userOrders = await db.Orders
            .Where(o => o.UserId == userId && o.Status == "Delivered")
            .Select(o => o.OrderId)
            .ToListAsync();

        return await db.OrderItems
            .AnyAsync(oi => userOrders.Contains(oi.OrderId) && oi.ProductId == productId);
    }

    // Review methods
    public async Task AddReview(Review review)
    {
        db.Reviews.Add(review);
        await db.SaveChangesAsync();
    }

    public async Task<List<Review>> GetProductReviews(Guid productId)
    {
        return await db.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetUserProductReview(Guid userId, Guid productId)
    {
        return await db.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<bool> DeleteReview(Guid reviewId)
    {
        var review = await db.Reviews.FindAsync(reviewId);
        if (review == null) return false;

        db.Reviews.Remove(review);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<double> GetProductAverageRating(Guid productId)
    {
        var reviews = await db.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (reviews.Count == 0) return 0;

        return reviews.Average(r => r.Rating);
    }

    // Wishlist methods
    public async Task AddToWishlist(Wishlist wishlist)
    {
        db.Wishlists.Add(wishlist);
        await db.SaveChangesAsync();
    }

    public async Task<List<Wishlist>> GetUserWishlist(Guid userId)
    {
        return await db.Wishlists
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }

    public async Task<Wishlist?> GetWishlistItem(Guid userId, Guid productId)
    {
        return await db.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<bool> RemoveFromWishlist(Guid wishlistId)
    {
        var wishlist = await db.Wishlists.FindAsync(wishlistId);
        if (wishlist == null) return false;

        db.Wishlists.Remove(wishlist);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearWishlist(Guid userId)
    {
        var wishlistItems = await GetUserWishlist(userId);
        db.Wishlists.RemoveRange(wishlistItems);
        await db.SaveChangesAsync();
        return true;
    }

    // Coupon methods
    public async Task CreateCoupon(Coupon coupon)
    {
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();
    }

    public async Task<Coupon?> GetCouponByCode(string code)
    {
        return await db.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower() && c.IsActive);
    }

    public async Task<List<Coupon>> GetAllCoupons()
    {
        return await db.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<bool> UpdateCouponUsage(Guid couponId)
    {
        var coupon = await db.Coupons.FindAsync(couponId);
        if (coupon == null) return false;

        coupon.UsedCount++;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateCoupon(Guid couponId)
    {
        var coupon = await db.Coupons.FindAsync(couponId);
        if (coupon == null) return false;

        coupon.IsActive = false;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelOrder(Guid orderId, string reason)
    {
        var order = await GetOrderById(orderId);
        if (order == null) return false;

        order.Status = "Cancelled";
        order.IsCancelled = true;
        order.CancellationReason = reason;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Restore product stock
        var orderItems = await GetOrderItems(orderId);
        foreach (var item in orderItems)
        {
            var product = await GetProductById(item.ProductId);
            if (product != null)
            {
                product.Stock += item.Quantity;
            }
        }

        await db.SaveChangesAsync();
        return true;
    }
}
