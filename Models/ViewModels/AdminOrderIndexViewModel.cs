using BanHang.Models;

namespace BanHang.Models.ViewModels
{
  public class AdminOrderIndexViewModel
  {
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();
    public int? StatusId { get; set; }
    public string? SearchTerm { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
  }
}