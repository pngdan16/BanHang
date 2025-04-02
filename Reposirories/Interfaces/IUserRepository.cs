using BanHang.Models.Identity;

namespace BanHang.Reposirories.Interfaces
{
    public interface IUserRepository
    {
        //Get all users 
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    }
}
