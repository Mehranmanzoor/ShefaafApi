namespace ShefaafAPI.Models;

public class DashboardStats
{
    // Overview
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public int LowStockCount { get; set; }
    
    // Order Status
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    
    // Today
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    
    // This Month
    public int MonthlyOrders { get; set; }
    public decimal MonthlyRevenue { get; set; }
    
    // Low Stock Products
    public List<LowStockProduct> LowStockProducts { get; set; } = new();
}

public class LowStockProduct
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
}