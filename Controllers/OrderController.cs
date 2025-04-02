using BanHang.Models;
using BanHang.Models.Identity;
using BanHang.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BanHang.Reposirories.Interfaces;

namespace BanHang.Controllers
{
  [Authorize]
  public class OrderController : Controller
  {
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderStatusRepository _orderStatusRepository;

    public OrderController(
        IOrderService orderService,
        UserManager<ApplicationUser> userManager,
        ILogger<OrderController> logger,
        IOrderStatusRepository orderStatusRepository)
    {
      _orderService = orderService;
      _userManager = userManager;
      _logger = logger;
      _orderStatusRepository = orderStatusRepository;
    }

    /// <summary>
    /// Hiển thị lịch sử đơn hàng của người dùng
    /// </summary>
    public async Task<IActionResult> History()
    {
      try
      {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
          return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("History", "Order") });
        }

        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return View(orders);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi lấy lịch sử đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi tải lịch sử đơn hàng";
        return RedirectToAction("Index", "Home");
      }
    }

    /// <summary>
    /// Hiển thị chi tiết đơn hàng
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
      try
      {
        var userId = _userManager.GetUserId(User);
        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null || order.UserId != userId)
        {
          return NotFound();
        }

        // Lấy danh sách trạng thái đơn hàng để hiển thị trong dropdown
        ViewBag.OrderStatuses = await _orderStatusRepository.GetAllAsync();

        return View(order);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi lấy chi tiết đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng";
        return RedirectToAction("History");
      }
    }

    /// <summary>
    /// Hủy đơn hàng (nếu chưa xử lý)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
      try
      {
        var userId = _userManager.GetUserId(User);
        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null || order.UserId != userId)
        {
          return NotFound();
        }

        // Thực hiện hủy đơn hàng
        await _orderService.CancelOrderAsync(id);

        TempData["Success"] = "Đã hủy đơn hàng thành công";
        return RedirectToAction("History");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi hủy đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi hủy đơn hàng";
        return RedirectToAction("History");
      }
    }

    /// <summary>
    /// Cập nhật trạng thái đơn hàng
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
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
        return RedirectToAction("Details", new { id = id });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái đơn hàng";
        return RedirectToAction("Details", new { id = id });
      }
    }
  }
}