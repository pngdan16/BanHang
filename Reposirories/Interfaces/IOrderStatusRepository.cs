using BanHang.Models;

namespace BanHang.Reposirories.Interfaces
{
  public interface IOrderStatusRepository
  { 
      // Lấy tất cả trạng thái đơn hàng 
    Task<IEnumerable<OrderStatus>> GetAllAsync();
    Task<OrderStatus> GetByIdAsync(int id);
    Task<OrderStatus> GetDefaultStatusAsync(); // Trả về trạng thái mặc định (Chờ xác nhận)
    Task<bool> IsValidTransitionAsync(int currentStatusId, int newStatusId);
    Task<OrderStatus> GetCancelledStatusAsync(); // Trả về trạng thái Đã hủy
  }
}