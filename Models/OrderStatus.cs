using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
  public class OrderStatus
  {
    public int Id { get; set; }
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
  }
}