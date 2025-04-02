using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
  /// <summary>
  /// Model đại diện cho một mục trong giỏ hàng của người dùng
  /// </summary>
  public class CartItem
  {
    /// <summary>
    /// ID của sản phẩm được thêm vào giỏ hàng
    /// </summary>
    public int ProductId { get; set; }
    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string Name { get; set; } = null!;



    /// <summary>
    /// Giá tiền của sản phẩm
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Số lượng sản phẩm trong giỏ hàng
    /// </summary>
    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
    public int Quantity { get; set; }
  }
}