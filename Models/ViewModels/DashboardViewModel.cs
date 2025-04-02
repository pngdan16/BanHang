using BanHang.Models;

namespace BanHang.Models.ViewModels
{
  public class DashboardViewModel
  {
    // Tổng đơn hàng
    public int TotalOrders { get; set; }

    // Tổng doanh số
    public decimal TotalSales { get; set; }

    // Đơn hàng gần đây
    public ICollection<Order> RecentOrders { get; set; } = new List<Order>();

    // Số liệu theo trạng thái
    public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
  }
}