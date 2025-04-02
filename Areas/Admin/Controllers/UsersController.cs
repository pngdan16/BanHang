using BanHang.Models.Identity;
using BanHang.Models.ViewModels;
using BanHang.Reposirories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.ADMIN + "," + RoleConstants.EMPLOYEE)]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        //GET: Admin/Users
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
        //GET: Admin/Users/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Log thông tin để debug
            _logger.LogInformation("User found: {UserId}, {UserName}, {Email}", user.Id, user.UserName, user.Email);

            var userRoles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User roles: {Roles}", string.Join(", ", userRoles));

            var model = new UserDetailsViewModel
            {
                User = user,
                Roles = userRoles
            };

            // Kiểm tra model trước khi trả về view
            if (model.User == null)
            {
                _logger.LogError("User object is null in model");
                return Problem("User object is null in model");
            }

            return View(model);
        }

        // GET: Admin/Users/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                AllRoles = roles,
                SelectedRoles = userRoles.ToList()
            };

            return View(model);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    user.Email = model.Email;
                    user.UserName = model.UserName;
                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.IsActive = model.IsActive;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Cập nhật vai trò
                        var userRoles = await _userManager.GetRolesAsync(user);

                        // Xóa các vai trò hiện tại
                        if (userRoles.Any())
                        {
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
                            if (!removeResult.Succeeded)
                            {
                                foreach (var error in removeResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }

                                // Lấy lại danh sách roles để hiển thị form
                                model.AllRoles = await _roleManager.Roles.ToListAsync();
                                return View(model);
                            }
                        }

                        // Thêm vai trò mới
                        if (model.SelectedRoles != null && model.SelectedRoles.Any())
                        {
                            var addResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                            if (!addResult.Succeeded)
                            {
                                foreach (var error in addResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }

                                // Lấy lại danh sách roles để hiển thị form
                                model.AllRoles = await _roleManager.Roles.ToListAsync();
                                return View(model);
                            }
                        }

                        _logger.LogInformation("Người dùng {UserName} đã được cập nhật thành công", user.UserName);
                        TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật người dùng {UserId}", id);
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật. Vui lòng thử lại.");
                }
            }

            // Nếu có lỗi, lấy lại danh sách roles để hiển thị form
            model.AllRoles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }
    }
}
