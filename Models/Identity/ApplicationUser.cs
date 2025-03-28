using Microsoft.AspNetCore.Identity;

namespace BanHang.Models.Identity
{
  // Mở rộng từ IdentityUser để thêm các trường tùy chỉnh
  public class ApplicationUser : IdentityUser
  {
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
  }
}