using BanHang.Extensions;
using BanHang.Models;
using BanHang.Models.Identity;
using BanHang.Reposirories.Interfaces;
using BanHang.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BanHang.Controllers
{
  public class ShoppingCartController : Controller
  {
    private readonly ApplicationDbContext _context;
    private const string CartSessionKey = "CartSession";
    private readonly IProductRepository _productRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOrderService _orderService;
    private readonly ILogger<ShoppingCartController> _logger;

    public ShoppingCartController(
        ApplicationDbContext context,
        IProductRepository productRepository,
        UserManager<ApplicationUser> userManager,
        IOrderService orderService,
        ILogger<ShoppingCartController> logger)
    {
      _context = context;
      _productRepository = productRepository;
      _userManager = userManager;
      _orderService = orderService;
      _logger = logger;
    }

    /// <summary>
    /// Hiển thị trang giỏ hàng
    /// </summary>
    public IActionResult Index()
    {
      // Lấy giỏ hàng từ session hoặc database nếu đã đăng nhập
      var cart = GetCart();
      return View(cart);
    }

    /// <summary>
    /// Thêm sản phẩm vào giỏ hàng
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
      try
      {
        // Lấy thông tin sản phẩm từ database
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
          TempData["Error"] = "Không tìm thấy sản phẩm";
          return RedirectToAction("Index", "Home");
        }

        // Lấy giỏ hàng 
        var cart = GetCart();

        // Tạo mới item để thêm vào giỏ hàng
        var item = new CartItem
        {
          ProductId = product.Id,
          Name = product.Name,
          Price = product.Price,
          Quantity = quantity
        };

        // Thêm vào giỏ hàng
        cart.AddItem(item);

        // Lưu giỏ hàng 
        SaveCart(cart);

        TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng";
        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
        return RedirectToAction("Index", "Home");
      }
    }

    /// <summary>
    /// Cập nhật số lượng sản phẩm trong giỏ hàng
    /// </summary>
    [HttpPost]
    public IActionResult UpdateCart(int productId, int quantity)
    {
      try
      {
        if (quantity < 1)
        {
          TempData["Error"] = "Số lượng phải lớn hơn 0";
          return RedirectToAction("Index");
        }

        var cart = GetCart();
        cart.UpdateQuantity(productId, quantity);
        SaveCart(cart);

        TempData["Success"] = "Đã cập nhật giỏ hàng";
        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
        return RedirectToAction("Index");
      }
    }

    /// <summary>
    /// Xóa sản phẩm khỏi giỏ hàng
    /// </summary>
    [HttpPost]
    public IActionResult RemoveFromCart(int productId)
    {
      try
      {
        var cart = GetCart();
        cart.RemoveItem(productId);
        SaveCart(cart);

        TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
        return RedirectToAction("Index");
      }
    }

    /// <summary>
    /// Xóa toàn bộ giỏ hàng
    /// </summary>
    public IActionResult ClearCart()
    {
      if (User.Identity.IsAuthenticated)
      {
        // Xử lý xóa giỏ hàng từ database
      }

      HttpContext.Session.Remove(CartSessionKey);
      TempData["Success"] = "Đã xóa toàn bộ giỏ hàng";
      return RedirectToAction("Index");
    }

    /// <summary>
    /// Chuyển đến trang thanh toán - yêu cầu đăng nhập
    /// </summary>
    [Authorize]
    public IActionResult Checkout()
    {
      var cart = GetCart();
      if (cart.IsEmpty())
      {
        TempData["Error"] = "Giỏ hàng trống, không thể thanh toán";
        return RedirectToAction("Index");
      }

      // Create a checkout view model with the cart data
      var checkoutViewModel = new CheckoutViewModel
      {
        Cart = cart
      };

      return View(checkoutViewModel);
    }

    /// <summary>
    /// Xử lý thanh toán đơn hàng
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          // Hiển thị chi tiết lỗi để debug
          var errors = string.Join("; ", ModelState.Values
              .SelectMany(v => v.Errors)
              .Select(e => e.ErrorMessage));

          _logger.LogWarning("Validation failed: {Errors}", errors);

          // Đảm bảo model có giỏ hàng
          model.Cart = GetCart();
          return View("Checkout", model);
        }

        // Lấy lại giỏ hàng từ session để đảm bảo dữ liệu mới nhất
        var cart = GetCart();

        if (cart == null || cart.IsEmpty())
        {
          TempData["Error"] = "Giỏ hàng trống, không thể thanh toán";
          return RedirectToAction("Index");
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
          TempData["Error"] = "Bạn cần đăng nhập để thanh toán";
          return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "ShoppingCart") });
        }

        // Sử dụng OrderService để tạo đơn hàng
        var order = await _orderService.CreateOrderFromCartAsync(
            cart,
            userId,
            model.ShippingAddress,
            model.Notes ?? ""
        );

        // Xóa giỏ hàng sau khi đặt hàng thành công
        HttpContext.Session.Remove(CartSessionKey);

        TempData["OrderSuccess"] = $"Đặt hàng thành công. Mã đơn hàng: {order.Id}";
        return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
      }
      catch (Exception ex)
      {
        TempData["Error"] = "Có lỗi xảy ra khi đặt hàng: " + ex.Message;

        // Ghi log error
        _logger.LogError(ex, "Order Error: {Message}", ex.Message);

        // Đảm bảo model có giỏ hàng khi có lỗi
        model.Cart = GetCart();

        return View("Checkout", model);
      }
    }

    /// <summary>
    /// Hiển thị trang xác nhận đơn hàng
    /// </summary>
    [Authorize]
    public async Task<IActionResult> OrderConfirmation(int orderId)
    {
      var userId = _userManager.GetUserId(User);

      // Sử dụng OrderService để lấy thông tin đơn hàng
      var order = await _orderService.GetOrderByIdAsync(orderId);

      if (order == null || order.UserId != userId)
      {
        return NotFound();
      }

      return View(order);
    }

    /// <summary>
    /// API lấy số lượng sản phẩm trong giỏ hàng
    /// </summary>
    [HttpGet]
    public IActionResult GetCartItemCount()
    {
      var cart = GetCart();
      int count = cart.GetTotalQuantity();
      return Json(count);
    }

    /// <summary>
    /// Lấy giỏ hàng (từ session nếu chưa đăng nhập, từ database nếu đã đăng nhập)
    /// </summary>
    private ShoppingCart GetCart()
    {
      // Lấy giỏ hàng từ session trước tiên
      var sessionCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);

      if (User.Identity.IsAuthenticated)
      {
        // Nếu đã đăng nhập và có giỏ hàng trong session
        if (sessionCart != null && !sessionCart.IsEmpty())
        {
          // Trả về giỏ hàng từ session (không xóa session)
          return sessionCart;
        }

        // Nếu không có giỏ hàng trong session, tìm kiếm trong database
        // TODO: Bổ sung logic lấy giỏ hàng từ database ở đây nếu cần

        // Nếu chưa có giỏ hàng, tạo mới và lưu vào session
        if (sessionCart == null)
        {
          var newCart = new ShoppingCart();
          SaveCartToSession(newCart);
          return newCart;
        }
      }

      // Nếu chưa đăng nhập, sử dụng session
      if (sessionCart == null)
      {
        sessionCart = new ShoppingCart();
        SaveCartToSession(sessionCart);
      }

      return sessionCart;
    }

    /// <summary>
    /// Lưu giỏ hàng vào session hoặc database
    /// </summary>
    private void SaveCart(ShoppingCart cart)
    {
      if (User.Identity.IsAuthenticated)
      {
        // Lưu vào database nếu đã đăng nhập
        // Xử lý lưu vào database ở đây
      }

      // Luôn lưu vào session
      SaveCartToSession(cart);
    }

    /// <summary>
    /// Lưu giỏ hàng vào session
    /// </summary>
    private void SaveCartToSession(ShoppingCart cart)
    {
      HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
    }
  }
}
