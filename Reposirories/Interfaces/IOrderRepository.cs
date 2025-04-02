using BanHang.Models;

namespace BanHang.Reposirories.Interfaces
{
  public interface IOrderRepository
  {
    // Lấy danh sách đơn hàng
    Task<IEnumerable<Order>> GetAllOrdersAsync();

    // Lấy đơn hàng theo id
    Task<Order> GetOrderByIdAsync(int id);

    // Lấy danh sách đơn hàng của người dùng
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);

    // Thêm đơn hàng mới
    Task<Order> CreateOrderAsync(Order order);

    // Cập nhật đơn hàng
    Task UpdateOrderAsync(Order order);

    // Xóa đơn hàng
    Task DeleteOrderAsync(int id);

    // Lấy chi tiết đơn hàng
    Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId);
  }
}