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
    public string ShippingAddress { get; set; }
    public string Notes { get; set; }
    // public string Status { get; set; }
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
    public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

  }
}
