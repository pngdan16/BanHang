using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
  public class CheckoutViewModel
  {
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
    [Display(Name = "Địa chỉ giao hàng")]
    public string ShippingAddress { get; set; }

    [Display(Name = "Ghi chú")]
    public string? Notes { get; set; }

    // Cart data for display - không bắt buộc trong validation
    [Display(Name = "Giỏ hàng")]
    public ShoppingCart Cart { get; set; } = new ShoppingCart(); // Khởi tạo mặc định
  }
}