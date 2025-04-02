using BanHang.Models;
using BanHang.Reposirories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Reposirories.Implementations
{
  public class OrderStatusRepository : IOrderStatusRepository
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderStatusRepository> _logger;

    public OrderStatusRepository(ApplicationDbContext context, ILogger<OrderStatusRepository> logger)
    {
          _context = context; 
          _logger = logger;
    }

    public async Task<IEnumerable<OrderStatus>> GetAllAsync()
    {
      return await _context.OrderStatuses.ToListAsync();
    }
    public async Task<OrderStatus> GetByIdAsync(int id)
    {
      var orderStatus = await _context.OrderStatuses.FindAsync(id);
      if (orderStatus == null)
      {
        throw new Exception("Không tìm thấy trạng thái đơn hàng");
      }
      return orderStatus;
    }
    public async Task<OrderStatus> GetDefaultStatusAsync()
    {
      return await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Id == 1);
    }
    public async Task<bool> IsValidTransitionAsync(int currentStatusId, int newStatusId)
    {
        _logger.LogInformation($"Checking transition from {currentStatusId} to {newStatusId}");
        // Lấy tất cả trạng thái
        var statuses = await GetAllAsync();

        // Lấy thông tin trạng thái hiện tại và mới
        var currentStatus = statuses.FirstOrDefault(s => s.Id == currentStatusId);
        var newStatus = statuses.FirstOrDefault(s => s.Id == newStatusId);

        if (currentStatus == null || newStatus == null) 
            return false;

        // Trạng thái "Đã hủy" (Id = 5) không thể chuyển sang trạng thái khác
        if (currentStatusId == 5) 
            return false;

        // Trạng thái "Đã hoàn thành" (Id = 4) chỉ có thể chuyển sang "Đã hủy" (Id = 5)
        if (currentStatusId == 4 && newStatusId != 5) 
            return false;

        // Không thể chuyển sang trạng thái có thứ tự hiển thị thấp hơn (ngoại trừ "Đã hủy")
        // if (newStatusId != 5 && currentStatus.DisplayOrder > newStatus.DisplayOrder)
        //   return false;

        return true;
    }
    public async Task<OrderStatus> GetCancelledStatusAsync()
    {
      return await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Id == 5);
    }



  }
}
