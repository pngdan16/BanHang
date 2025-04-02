using System.ComponentModel.DataAnnotations.Schema;
using BanHang.Models.Identity;

namespace BanHang.Models
{
  public class Order
  {
    public int Id { get; set; }
    public string UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    // Thời gian cập nhật trạng thái gần nhất
    public DateTime? StatusUpdatedDate { get; set; }
    public int OrderStatusId { get; set; }
    [ForeignKey("OrderStatusId")]
    public OrderStatus OrderStatus { get; set; } = null!;
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
    public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

  }
}
