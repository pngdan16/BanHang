using BanHang.DTO.Product;
using BanHang.Models;
using BanHang.Reposirories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BanHang.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(
        IProductRepository productRepository, 
        ICategoryRepository categoryRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        // Load danh sách categories cho dropdown
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(new ProductViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ProductViewModel productVM)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload hình ảnh chính
                if (productVM.ImageFile != null)
                {
                    productVM.ImageUrl = await UploadImage(productVM.ImageFile);
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh chính");
                    await PrepareCategories();
                    return View(productVM);
                }

                // Xử lý upload hình ảnh bổ sung
                if (productVM.AdditionalImages != null && productVM.AdditionalImages.Any())
                {
                    foreach (var image in productVM.AdditionalImages.Where(i => i != null && i.Length > 0))
                    {
                        var imageUrl = await UploadImage(image);
                        productVM.AdditionalImageUrls.Add(imageUrl);
                    }
                }

                // Lưu sản phẩm vào database
                await _productRepository.AddAsync(productVM);
                
                TempData["Success"] = "Sản phẩm đã được thêm thành công";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sản phẩm: " + ex.Message);
        }

        // Nếu ModelState không hợp lệ hoặc có lỗi, load lại danh sách categories
        await PrepareCategories();
        return View(productVM);
    }

    #region Helper Methods
    private async Task<string> UploadImage(IFormFile imageFile)
    {
        // Tạo đường dẫn thư mục uploads
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
        
        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Tạo tên file độc nhất
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Lưu file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(fileStream);
        }

        // Trả về đường dẫn tương đối
        return "/images/products/" + uniqueFileName;
    }
      
    private async Task PrepareCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
    }
    #endregion
}
