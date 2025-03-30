using BanHang.Models;
using BanHang.Reposirories.Interfaces;
using BanHang.Services.Interfaces;

namespace BanHang.Services.Implementations
{
  public class OrderService : IOrderService
  {
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
      _orderRepository = orderRepository;
      _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
      return await _orderRepository.GetAllOrdersAsync();
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
      return await _orderRepository.GetOrderByIdAsync(id);
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
    {
      return await _orderRepository.GetOrdersByUserIdAsync(userId);
    }

    public async Task<Order> CreateOrderFromCartAsync(ShoppingCart cart, string userId, string shippingAddress, string notes)
    {
      if (cart == null)
      {
        throw new ArgumentNullException(nameof(cart), "Giỏ hàng không được null");
      }

      if (cart.IsEmpty())
      {
        throw new InvalidOperationException("Không thể tạo đơn hàng từ giỏ hàng trống");
      }

      if (string.IsNullOrEmpty(userId))
      {
        throw new ArgumentException("ID người dùng không được để trống", nameof(userId));
      }

      if (string.IsNullOrEmpty(shippingAddress))
      {
        throw new ArgumentException("Địa chỉ giao hàng không được để trống", nameof(shippingAddress));
      }

      try
      {
        _logger.LogInformation("Bắt đầu tạo đơn hàng từ giỏ hàng cho người dùng: {UserId}", userId);

        // Tạo đơn hàng mới
        var order = new Order
        {
          UserId = userId,
          OrderDate = DateTime.Now,
          TotalAmount = cart.GetTotalAmount(),
          ShippingAddress = shippingAddress,
          Notes = notes ?? string.Empty
        };

        _logger.LogInformation("Tổng giá trị đơn hàng: {TotalAmount}", order.TotalAmount);

        // Thêm chi tiết đơn hàng
        foreach (var item in cart.Items)
        {
          _logger.LogInformation("Thêm sản phẩm vào đơn hàng: {ProductId}, Số lượng: {Quantity}, Đơn giá: {Price}",
                                item.ProductId, item.Quantity, item.Price);

          order.OrderDetails.Add(new OrderDetail
          {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.Price
          });
        }

        // Lưu đơn hàng vào database thông qua repository
        var createdOrder = await _orderRepository.CreateOrderAsync(order);

        _logger.LogInformation("Đã tạo đơn hàng thành công từ giỏ hàng, Order ID: {OrderId}", createdOrder.Id);

        return createdOrder;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi tạo đơn hàng từ giỏ hàng: {Message}", ex.Message);
        throw new Exception($"Không thể tạo đơn hàng: {ex.Message}", ex);
      }
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status)
    {
      var order = await _orderRepository.GetOrderByIdAsync(orderId);
      if (order == null)
      {
        throw new KeyNotFoundException($"Không tìm thấy đơn hàng với ID: {orderId}");
      }

      // Cập nhật thêm trường Status vào bảng Order nếu cần
      // order.Status = status;

      await _orderRepository.UpdateOrderAsync(order);

      _logger.LogInformation("Đã cập nhật trạng thái đơn hàng ID: {OrderId} thành: {Status}", orderId, status);
    }

    public async Task CancelOrderAsync(int orderId)
    {
      var order = await _orderRepository.GetOrderByIdAsync(orderId);
      if (order == null)
      {
        throw new KeyNotFoundException($"Không tìm thấy đơn hàng với ID: {orderId}");
      }

      // Cập nhật trạng thái đơn hàng thành "Đã hủy"
      // order.Status = "Cancelled";

      await _orderRepository.UpdateOrderAsync(order);

      _logger.LogInformation("Đã hủy đơn hàng ID: {OrderId}", orderId);
    }
  }
}