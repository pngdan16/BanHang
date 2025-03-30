using BanHang.Models;
using BanHang.Reposirories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Reposirories.Implementations
{
  public class OrderRepository : IOrderRepository
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(ApplicationDbContext context, ILogger<OrderRepository> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
      return await _context.Orders
          .Include(o => o.OrderDetails)
          .ThenInclude(od => od.Product)
          .Include(o => o.User)
          .OrderByDescending(o => o.OrderDate)
          .ToListAsync();
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
      return await _context.Orders
          .Include(o => o.OrderDetails)
          .ThenInclude(od => od.Product)
          .Include(o => o.User)
          .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
    {
      return await _context.Orders
          .Include(o => o.OrderDetails)
          .ThenInclude(od => od.Product)
          .Where(o => o.UserId == userId)
          .OrderByDescending(o => o.OrderDate)
          .ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
      _logger.LogInformation("Tạo đơn hàng mới cho người dùng: {UserId}", order.UserId);

      // Dùng transaction để đảm bảo tính toàn vẹn của dữ liệu
      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
        _logger.LogInformation("Đã tạo đơn hàng thành công với ID: {OrderId}", order.Id);

        return order;
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Lỗi khi tạo đơn hàng cho người dùng {UserId}: {Message}", order.UserId, ex.Message);
        throw;
      }
    }

    public async Task UpdateOrderAsync(Order order)
    {
      _logger.LogInformation("Cập nhật đơn hàng ID: {OrderId}", order.Id);

      _context.Orders.Update(order);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteOrderAsync(int id)
    {
      _logger.LogInformation("Xóa đơn hàng ID: {OrderId}", id);

      var order = await _context.Orders.FindAsync(id);
      if (order != null)
      {
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
      }
    }

    public async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId)
    {
      return await _context.OrderDetails
          .Include(od => od.Product)
          .Where(od => od.OrderId == orderId)
          .ToListAsync();
    }
  }
}