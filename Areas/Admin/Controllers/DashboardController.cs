using BanHang.Models;
using BanHang.Models.Identity;
using BanHang.Reposirories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BanHang.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize(Roles = $"{RoleConstants.ADMIN},{RoleConstants.EMPLOYEE}")]
  public class DashboardController : Controller
  {
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    public DashboardController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
      _productRepository = productRepository;
      _categoryRepository = categoryRepository;
    }

    // Trang Dashboard chính
    public async Task<IActionResult> Index()
    {
      try
      {
        // Lấy thông tin tổng quan
        var productCount = await _productRepository.GetAllAsync();
        var categories = await _categoryRepository.GetAllAsync();

        // Truyền dữ liệu tổng quan vào ViewBag
        ViewBag.ProductCount = productCount.Count();
        ViewBag.CategoryCount = categories.Count();

        return View();
      }
      catch (Exception ex)
      {
        // Xử lý lỗi và hiển thị thông báo
        TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
        return View();
      }
    }

    // Quản lý sản phẩm
    public async Task<IActionResult> Products(int? categoryId)
    {
      try
      {
        var products = await _productRepository.GetAllAsync();

        // Lọc sản phẩm theo danh mục nếu có categoryId
        if (categoryId.HasValue)
        {
          products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

          // Lấy thông tin danh mục để hiển thị trong view
          var category = await _categoryRepository.GetByIdAsync(categoryId.Value);
          if (category != null)
          {
            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = category.Id;
          }
        }

        return View(products);
      }
      catch (Exception ex)
      {
        TempData["ErrorMessage"] = $"Không thể tải danh sách sản phẩm: {ex.Message}";
        return View(new List<Product>());
      }
    }

    // Quản lý danh mục
    public async Task<IActionResult> Categories()
    {
      try
      {
        var categories = await _categoryRepository.GetAllAsync();
        return View(categories);
      }
      catch (Exception ex)
      {
        TempData["ErrorMessage"] = $"Không thể tải danh sách danh mục: {ex.Message}";
        return View(new List<Category>());
      }
    }
  }
}
