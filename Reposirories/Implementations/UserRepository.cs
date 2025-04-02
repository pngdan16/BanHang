using BanHang.Models.Identity;
using BanHang.Reposirories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Reposirories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> userManager;


        public UserRepository(UserManager<ApplicationUser> _userManager)
        {
            _userManager = userManager; 
        }
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await userManager.Users.ToListAsync();
        }
    }
}
