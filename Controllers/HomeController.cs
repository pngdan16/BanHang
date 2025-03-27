using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BanHang.Models;
using BanHang.Reposirories;
using BanHang.Reposirories.Interfaces;

namespace BanHang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    public HomeController(ILogger<HomeController> logger, IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }
    //Hiển thị chi tiết sản phẩm
    public async Task<IActionResult> ProductDetail(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if(product == null)
        {
            return NotFound();
        }
        return View(product);
    }
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
