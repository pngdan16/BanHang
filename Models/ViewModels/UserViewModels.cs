using BanHang.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace BanHang.Models.ViewModels
{
  public class UserDetailsViewModel
  {
    public ApplicationUser User { get; set; } = new ApplicationUser();
    public IList<string> Roles { get; set; } = new List<string>();
  }
  public class UserEditViewModel
  {
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public IEnumerable<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();
    public List<string> SelectedRoles { get; set; } = new List<string>();
  }
}