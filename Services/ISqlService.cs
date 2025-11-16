using ShefaafAPI.Models;

namespace ShefaafAPI.Services;

public interface ISqlService
{
    // User methods
    Task<User?> FindUser(string email);
    Task<User?> FindUserById(Guid userId);
    Task<List<User>> GetAllUsers();
    Task CreateUser(User user);
    Task<bool> DeleteUser(string email);
    Task<bool> UpdatePass(Guid userId, string newPassword);
    Task<bool> UpdateUserRole(Guid userId, string role);
    
    // Product methods
    Task<List<Product>> GetAllProducts();
    Task<Product?> GetProductById(Guid productId);
    Task<List<Product>> GetProductsByCategory(string category);
    Task<List<Product>> SearchProducts(string searchTerm);
    Task CreateProduct(Product product);
    Task<bool> UpdateProduct(Product product);
    Task<bool> DeleteProduct(Guid productId);
    Task<bool> UpdateProductStock(Guid productId, int stock);
    
    // Cart methods
    Task<List<Cart>> GetUserCart(Guid userId);
    Task<Cart?> GetCartItem(Guid userId, Guid productId);
    Task AddToCart(Cart cart);
    Task<bool> UpdateCartQuantity(Guid cartId, int quantity);
    Task<bool> RemoveFromCart(Guid cartId);
    Task<bool> ClearCart(Guid userId);
    
    // Order methods
    Task CreateOrder(Order order);
    Task CreateOrderItem(OrderItem orderItem);
    Task<List<Order>> GetUserOrders(Guid userId);
    Task<Order?> GetOrderById(Guid orderId);
    Task<List<OrderItem>> GetOrderItems(Guid orderId);
    Task<bool> UpdateOrderStatus(Guid orderId, string status);
    Task<List<Order>> GetAllOrders();
    Task<bool> HasUserPurchasedProduct(Guid userId, Guid productId);
    Task<bool> CancelOrder(Guid orderId, string reason);
    
    // Review methods
    Task AddReview(Review review);
    Task<List<Review>> GetProductReviews(Guid productId);
    Task<Review?> GetUserProductReview(Guid userId, Guid productId);
    Task<bool> DeleteReview(Guid reviewId);
    Task<double> GetProductAverageRating(Guid productId);
    
    // Wishlist methods
    Task AddToWishlist(Wishlist wishlist);
    Task<List<Wishlist>> GetUserWishlist(Guid userId);
    Task<Wishlist?> GetWishlistItem(Guid userId, Guid productId);
    Task<bool> RemoveFromWishlist(Guid wishlistId);
    Task<bool> ClearWishlist(Guid userId);
    
    // Coupon methods
    Task CreateCoupon(Coupon coupon);
    Task<Coupon?> GetCouponByCode(string code);
    Task<List<Coupon>> GetAllCoupons();
    Task<bool> UpdateCouponUsage(Guid couponId);
    Task<bool> DeactivateCoupon(Guid couponId);
}
