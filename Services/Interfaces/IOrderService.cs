using BanHang.Models;

namespace BanHang.Services.Interfaces
{
  public interface IOrderService
  {
    // Lấy danh sách đơn hàng
    Task<IEnumerable<Order>> GetAllOrdersAsync();

    // Lấy đơn hàng theo id
    Task<Order> GetOrderByIdAsync(int id);

    // Lấy danh sách đơn hàng của người dùng
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);

    // Tạo đơn hàng từ giỏ hàng
    Task<Order> CreateOrderFromCartAsync(ShoppingCart cart, string userId, string shippingAddress, string notes);

    // Cập nhật trạng thái đơn hàng
    Task UpdateOrderStatusAsync(int orderId, string status);

    // Cập nhật trạng thái đơn hàng theo statusId
    Task UpdateOrderStatusAsync(int orderId, int statusId);

    // Hủy đơn hàng
    Task CancelOrderAsync(int orderId);
  }
}