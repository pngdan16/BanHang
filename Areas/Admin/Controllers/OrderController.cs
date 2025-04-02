using BanHang.Models;
using BanHang.Models.Identity;
using BanHang.Models.ViewModels;
using BanHang.Reposirories.Interfaces;
using BanHang.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize(Roles = RoleConstants.ADMIN + "," + RoleConstants.EMPLOYEE)]
  public class OrderController : Controller
  {
    private readonly IOrderService _orderService;
    private readonly IOrderStatusRepository _orderStatusRepository;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        IOrderStatusRepository orderStatusRepository,
        ILogger<OrderController> logger)
    {
      _orderService = orderService;
      _orderStatusRepository = orderStatusRepository;
      _logger = logger;
    }

    // GET: Admin/Order
    public async Task<IActionResult> Index(int? statusId, string? searchTerm, int page = 1)
    {
      try
      {
        int pageSize = 10;
        var allOrders = await _orderService.GetAllOrdersAsync();
        var statuses = await _orderStatusRepository.GetAllAsync();

        // Lọc theo trạng thái nếu có
        if (statusId.HasValue && statusId > 0)
        {
          allOrders = allOrders.Where(o => o.OrderStatusId == statusId.Value);
        }

        // Lọc theo từ khóa nếu có (mã đơn hàng hoặc địa chỉ)
        if (!string.IsNullOrEmpty(searchTerm))
        {
          searchTerm = searchTerm.ToLower();
          allOrders = allOrders.Where(o =>
            o.Id.ToString().Contains(searchTerm) ||
            o.ShippingAddress.ToLower().Contains(searchTerm) ||
            (o.User != null && o.User.UserName.ToLower().Contains(searchTerm))
          );
        }

        // Sắp xếp theo ngày đặt hàng giảm dần (mới nhất lên đầu)
        var orderedList = allOrders.OrderByDescending(o => o.OrderDate).ToList();

        // Phân trang
        var paginatedOrders = orderedList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Tính tổng số trang
        int totalItems = orderedList.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Tạo view model
        var viewModel = new AdminOrderIndexViewModel
        {
          Orders = paginatedOrders,
          OrderStatuses = statuses.ToList(),
          CurrentPage = page,
          TotalPages = totalPages,
          StatusId = statusId,
          SearchTerm = searchTerm
        };

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi tải danh sách đơn hàng";
        return View(new AdminOrderIndexViewModel());
      }
    }

    // GET: Admin/Order/Details/5
    public async Task<IActionResult> Details(int id)
    {
      try
      {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
          return NotFound();
        }

        ViewBag.OrderStatuses = await _orderStatusRepository.GetAllAsync();
        return View(order);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi lấy chi tiết đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng";
        return RedirectToAction(nameof(Index));
      }
    }

    // POST: Admin/Order/UpdateStatus/5
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, int statusId)
    {
      try
      {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
          return NotFound();
        }

        await _orderService.UpdateOrderStatusAsync(id, statusId);

        TempData["Success"] = "Đã cập nhật trạng thái đơn hàng thành công";
        return RedirectToAction(nameof(Details), new { id = id });
      }
      catch (InvalidOperationException ex)
      {
        _logger.LogWarning(ex, "Không thể cập nhật trạng thái đơn hàng ID {OrderId}: {Message}", id, ex.Message);
        TempData["Error"] = $"Không thể cập nhật trạng thái đơn hàng: {ex.Message}";
        return RedirectToAction(nameof(Details), new { id = id });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái đơn hàng";
        return RedirectToAction(nameof(Details), new { id = id });
      }
    }

    // POST: Admin/Order/Cancel/5
    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
      try
      {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
          return NotFound();
        }

        await _orderService.CancelOrderAsync(id);

        TempData["Success"] = "Đã hủy đơn hàng thành công";
        return RedirectToAction(nameof(Details), new { id = id });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi hủy đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi hủy đơn hàng";
        return RedirectToAction(nameof(Details), new { id = id });
      }
    }
  }
}