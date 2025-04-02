using BanHang.Models;
using BanHang.Models.Identity;
using BanHang.Models.ViewModels;
using BanHang.Reposirories.Interfaces;
using BanHang.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace BanHang.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize(Roles = RoleConstants.ADMIN + "," + RoleConstants.EMPLOYEE)]
  public class DashboardController : Controller
  {
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IOrderService _orderService;
    private readonly IOrderStatusRepository _orderStatusRepository;
    private readonly ILogger<DashboardController> _logger;
    private readonly IUserRepository _userRepository;

    public DashboardController(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IOrderService orderService,
    IOrderStatusRepository orderStatusRepository,
    IUserRepository userRepository,
    ILogger<DashboardController> logger)
    {
      _productRepository = productRepository;
      _categoryRepository = categoryRepository;
      _orderService = orderService;
      _orderStatusRepository = orderStatusRepository;
      _userRepository = userRepository;
      _logger = logger;
    }

    // Giao diện trang Dashboard chính của Admin
    public async Task<IActionResult> Index()
    {
      try
      {
        // Lấy số liệu tổng quan về đơn hàng
        var allOrders = await _orderService.GetAllOrdersAsync();
        // Tính tổng số đơn hàng  
        var totalOrders = allOrders.Count();
        // Tính tổng doanh số 
        var totalSales = allOrders.Sum(o => o.TotalAmount);

        // Lấy danh sách đơn hàng mới nhất (5 đơn)
        var recentOrders = allOrders.OrderByDescending(o => o.OrderDate).Take(5).ToList();

        // Lấy số liệu theo trạng thái đơn hàng
        var statuses = await _orderStatusRepository.GetAllAsync();
        var ordersByStatus = new Dictionary<string, int>();

        foreach (var status in statuses)
        {
          var count = allOrders.Count(o => o.OrderStatusId == status.Id);
          ordersByStatus.Add(status.Name, count);
        }

        // Tạo view model cho dashboard
        var viewModel = new DashboardViewModel
        {
          TotalOrders = totalOrders,
          TotalSales = totalSales,
          RecentOrders = recentOrders,
          OrdersByStatus = ordersByStatus
        };

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi tải trang Dashboard: {Message}", ex.Message);
        TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu tổng quan";
        return View(new DashboardViewModel());
      }
    }

    // Quản lý sản phẩm
    public async Task<IActionResult> Products(int? categoryId)
    {
      try
      {
        var products = await _productRepository.GetAllAsync();

        // Lọc sản phẩm theo danh mục nếu có categoryId
        if (categoryId.HasValue)
        {
          products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

          // Lấy thông tin danh mục để hiển thị trong view
          var category = await _categoryRepository.GetByIdAsync(categoryId.Value);
          if (category != null)
          {
            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = category.Id;
          }
        }

        return View(products);
      }
      catch (Exception ex)
      {
        TempData["ErrorMessage"] = $"Không thể tải danh sách sản phẩm: {ex.Message}";
        return View(new List<Product>());
      }
    }

    // Quản lý danh mục
    public async Task<IActionResult> Categories()
    {
      try
      {
        var categories = await _categoryRepository.GetAllAsync();
        return View(categories);
      }
      catch (Exception ex)
      {
        TempData["ErrorMessage"] = $"Không thể tải danh sách danh mục: {ex.Message}";
        return View(new List<Category>());
      }
    }
    //Quản lý người dùng 
    public async Task<IActionResult> Users()
    {
      try
      {
        var users = await _userRepository.GetAllUsersAsync();
        return View(users);
      }
      catch (Exception ex)
      {
        TempData["ErrorMessage"] = $"Không thể tải danh sách người dùng: {ex.Message}";
        return View(new List<ApplicationUser>());
      }
    }
  }
}
