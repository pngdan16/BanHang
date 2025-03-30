using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
  public class OrderDetail
  {
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    [ForeignKey("OrderId")]
    public Order Order { get; set; } = null!;
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
  }
}
