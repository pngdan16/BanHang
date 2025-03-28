using BanHang.DTO.Product;
using BanHang.Models;
using BanHang.Reposirories.Interfaces;
using BanHang.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace BanHang.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IWebHostEnvironment webHostEnvironment,
        ICloudinaryService cloudinaryService,
        ILogger<ProductController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Hiển thị trang chủ
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }
    /// <summary>
    /// Hiển thị chi tiết sản phẩm
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
    /// <summary>
    /// Hiển thị trang thêm sản phẩm
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Add()
    {
        // Load danh sách categories cho dropdown
        await PrepareCategories();
        return View(new ProductViewModel());
    }
    /// <summary>
    /// Thêm sản phẩm
    /// </summary>
    /// <param name="productVM"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ProductViewModel productVM)
    {
        try
        {
            // Xác thực ImageFile ngoài ModelState để kiểm soát tốt hơn
            if (productVM.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh chính");
            }
            else if (!IsValidImageFile(productVM.ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Chỉ chấp nhận các định dạng hình ảnh: jpg, jpeg, png, gif");
            }
            else if (productVM.ImageFile.Length > 10 * 1024 * 1024) // 10MB
            {
                ModelState.AddModelError("ImageFile", "Kích thước hình ảnh không được vượt quá 10MB");
            }

            // Kiểm tra hình ảnh bổ sung
            if (productVM.AdditionalImages != null && productVM.AdditionalImages.Any(i => i != null && i.Length > 0))
            {
                StringBuilder errorMsg = new StringBuilder();
                foreach (var image in productVM.AdditionalImages.Where(i => i != null && i.Length > 0))
                {
                    if (!IsValidImageFile(image))
                    {
                        errorMsg.AppendLine($"Hình ảnh '{image.FileName}' không đúng định dạng. Chỉ chấp nhận: jpg, jpeg, png, gif");
                    }
                    else if (image.Length > 10 * 1024 * 1024) // 10MB
                    {
                        errorMsg.AppendLine($"Hình ảnh '{image.FileName}' vượt quá kích thước cho phép (10MB)");
                    }
                }

                if (errorMsg.Length > 0)
                {
                    ModelState.AddModelError("AdditionalImages", errorMsg.ToString());
                }
            }

            // Kiểm tra ModelState sau khi đã thêm các lỗi tùy chỉnh
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model không hợp lệ: {Errors}",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                await PrepareCategories();
                return View(productVM);
            }

            // Đảm bảo ImageFile không null (đã kiểm tra bên trên)
            if (productVM.ImageFile != null)
            {
                try
                {
                    var imageResult = await _cloudinaryService.UploadImageAsync(productVM.ImageFile);
                    if (imageResult != null)
                    {
                        productVM.ImageUrl = imageResult.SecureUrl.ToString();
                        _logger.LogInformation("Tải lên hình ảnh chính thành công: {Url}", productVM.ImageUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("ImageFile", "Không thể tải lên hình ảnh chính");
                        await PrepareCategories();
                        return View(productVM);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tải lên hình ảnh chính");
                    ModelState.AddModelError("ImageFile", $"Lỗi tải lên hình ảnh: {ex.Message}");
                    await PrepareCategories();
                    return View(productVM);
                }
            }

            // Xử lý upload hình ảnh bổ sung lên Cloudinary
            if (productVM.AdditionalImages != null && productVM.AdditionalImages.Any(i => i != null && i.Length > 0))
            {
                foreach (var image in productVM.AdditionalImages.Where(i => i != null && i.Length > 0))
                {
                    try
                    {
                        var imageResult = await _cloudinaryService.UploadImageAsync(image);
                        if (imageResult != null)
                        {
                            productVM.AdditionalImageUrls.Add(imageResult.SecureUrl.ToString());
                            _logger.LogInformation("Tải lên hình ảnh bổ sung thành công: {Url}", imageResult.SecureUrl.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi tải lên hình ảnh bổ sung: {FileName}", image.FileName);
                        // Tiếp tục với các hình ảnh khác nếu có lỗi
                    }
                }
            }

            // Lưu sản phẩm vào database
            try
            {
                await _productRepository.AddAsync(productVM);
                _logger.LogInformation("Thêm sản phẩm thành công: {ProductName}", productVM.Name);

                TempData["Success"] = "Sản phẩm đã được thêm thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu sản phẩm vào database: {Error}", ex.Message);
                ModelState.AddModelError("", $"Lỗi khi lưu sản phẩm: {ex.Message}");
                await PrepareCategories();
                return View(productVM);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi thêm sản phẩm");
            ModelState.AddModelError("", $"Có lỗi xảy ra khi thêm sản phẩm: {ex.Message}");
            await PrepareCategories();
            return View(productVM);
        }
    }
    /// <summary>
    /// Hiển thị trang chỉnh sửa sản phẩm
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Bắt đầu chỉnh sửa sản phẩm với ID: {ProductId}", id);

        // Lấy thông tin sản phẩm từ database
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Không tìm thấy sản phẩm với ID: {ProductId}", id);
            return NotFound();
        }

        // Chuyển đổi từ Model sang ViewModel
        var productVM = new ProductViewModel
        {
            Name = product.Name,
            Price = product.Price,
            Description = product.Description ?? string.Empty,
            CategoryId = product.CategoryId,
            ImageUrl = product.ImageUrl ?? string.Empty
        };

        // Thêm các hình ảnh bổ sung nếu có
        if (product.Images != null && product.Images.Any())
        {
            productVM.AdditionalImageUrls = product.Images.Select(img => img.Url).ToList();
            // Truyền danh sách đối tượng ProductImage vào ViewBag để có thể xóa các hình ảnh
            ViewBag.ProductImages = product.Images.ToList();
        }

        // Load danh sách categories cho dropdown
        await PrepareCategories();

        // Truyền id vào ViewBag để sử dụng trong form
        ViewBag.ProductId = id;

        return View(productVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel productVM)
    {


        // Kiểm tra tồn tại
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            _logger.LogWarning("Không tìm thấy sản phẩm với ID: {ProductId} khi cập nhật", id);
            return NotFound();
        }

        // Xác thực hình ảnh nếu có
        if (productVM.ImageFile != null)
        {
            if (!IsValidImageFile(productVM.ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Chỉ chấp nhận các định dạng hình ảnh: jpg, jpeg, png, gif, webp");
            }
            else if (productVM.ImageFile.Length > 10 * 1024 * 1024) // 10MB
            {
                ModelState.AddModelError("ImageFile", "Kích thước hình ảnh không được vượt quá 10MB");
            }
        }

        // Kiểm tra hình ảnh bổ sung
        if (productVM.AdditionalImages != null && productVM.AdditionalImages.Any(i => i != null && i.Length > 0))
        {
            StringBuilder errorMsg = new StringBuilder();
            foreach (var image in productVM.AdditionalImages.Where(i => i != null && i.Length > 0))
            {
                if (!IsValidImageFile(image))
                {
                    errorMsg.AppendLine($"Hình ảnh '{image.FileName}' không đúng định dạng. Chỉ chấp nhận: jpg, jpeg, png, gif, webp");
                }
                else if (image.Length > 10 * 1024 * 1024) // 10MB
                {
                    errorMsg.AppendLine($"Hình ảnh '{image.FileName}' vượt quá kích thước cho phép (10MB)");
                }
            }

            if (errorMsg.Length > 0)
            {
                ModelState.AddModelError("AdditionalImages", errorMsg.ToString());
            }
        }

        // Kiểm tra ModelState sau khi đã thêm các lỗi tùy chỉnh
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model không hợp lệ khi cập nhật sản phẩm: {ProductId}, Errors: {Errors}",
                id, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            await PrepareCategories();
            ViewBag.ProductId = id;
            return View(productVM);
        }

        // Xử lý trong transaction để đảm bảo toàn vẹn dữ liệu
        try
        {
            // Cập nhật thông tin cơ bản của sản phẩm
            existingProduct.Name = productVM.Name;
            existingProduct.Price = productVM.Price;
            existingProduct.Description = productVM.Description;
            existingProduct.CategoryId = productVM.CategoryId;

            // Xử lý hình ảnh chính nếu có thay đổi
            if (productVM.ImageFile != null)
            {
                try
                {
                    var imageResult = await _cloudinaryService.UploadImageAsync(productVM.ImageFile);
                    if (imageResult != null)
                    {
                        // Lưu URL hình ảnh mới
                        existingProduct.ImageUrl = imageResult.SecureUrl.ToString();
                        _logger.LogInformation("Đã cập nhật hình ảnh chính cho sản phẩm {ProductId}", id);
                    }
                    else
                    {
                        ModelState.AddModelError("ImageFile", "Không thể tải lên hình ảnh chính");
                        await PrepareCategories();
                        ViewBag.ProductId = id;
                        return View(productVM);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tải lên hình ảnh chính cho sản phẩm {ProductId}", id);
                    ModelState.AddModelError("ImageFile", $"Lỗi tải lên hình ảnh: {ex.Message}");
                    await PrepareCategories();
                    ViewBag.ProductId = id;
                    return View(productVM);
                }
            }

            // Xử lý các hình ảnh bổ sung mới nếu có
            var newAdditionalImageUrls = new List<string>();
            if (productVM.AdditionalImages != null && productVM.AdditionalImages.Any(i => i != null && i.Length > 0))
            {
                foreach (var image in productVM.AdditionalImages.Where(i => i != null && i.Length > 0))
                {
                    try
                    {
                        var imageResult = await _cloudinaryService.UploadImageAsync(image);
                        if (imageResult != null)
                        {
                            newAdditionalImageUrls.Add(imageResult.SecureUrl.ToString());
                            _logger.LogInformation("Đã tải lên hình ảnh bổ sung mới cho sản phẩm {ProductId}", id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi tải lên hình ảnh bổ sung mới cho sản phẩm {ProductId}", id);
                        // Tiếp tục với các hình ảnh khác nếu có lỗi
                    }
                }
            }

            // Thêm các hình ảnh bổ sung mới vào danh sách
            if (existingProduct.Images == null)
            {
                existingProduct.Images = new List<ProductImage>();
            }

            foreach (var imageUrl in newAdditionalImageUrls)
            {
                existingProduct.Images.Add(new ProductImage
                {
                    ProductId = id,
                    Url = imageUrl
                });
            }

            // Cập nhật sản phẩm trong database
            await _productRepository.UpdateAsync(existingProduct);

            TempData["Success"] = "Sản phẩm đã được cập nhật thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm {ProductId}: {Error}", id, ex.Message);
            ModelState.AddModelError("", $"Lỗi khi cập nhật sản phẩm: {ex.Message}");
            await PrepareCategories();
            ViewBag.ProductId = id;
            return View(productVM);
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProductImage(int productId, int imageId)
    {
        _logger.LogInformation("Xóa hình ảnh {ImageId} của sản phẩm {ProductId}", imageId, productId);

        var result = await _productRepository.DeleteProductImageAsync(productId, imageId);

        if (!result)
        {
            _logger.LogWarning("Không thể xóa hình ảnh {ImageId} của sản phẩm {ProductId}", imageId, productId);
            TempData["Error"] = "Không thể xóa hình ảnh này";
        }
        else
        {
            _logger.LogInformation("Đã xóa hình ảnh {ImageId} của sản phẩm {ProductId} thành công", imageId, productId);
            TempData["Success"] = "Đã xóa hình ảnh thành công";
        }

        return RedirectToAction("Edit", new { id = productId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Hiển thị trang xác nhận xóa sản phẩm: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Không tìm thấy sản phẩm với ID: {ProductId} khi cần xóa", id);
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation("Starting deletion process for product with ID: {productId}", id);

        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {productId} not found for deletion", id);
                TempData["Error"] = "Không tìm thấy sản phẩm để xóa";
                return RedirectToAction(nameof(Index));
            }

            await _productRepository.DeleteAsync(id);

            _logger.LogInformation("Successfully deleted product with ID: {productId}", id);
            TempData["Success"] = "Xóa sản phẩm thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {productId}", id);
            TempData["Error"] = $"Lỗi khi xóa sản phẩm: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #region Helper Methods
    private async Task PrepareCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
    }

    private bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => true,
            _ => false
        };
    }
    #endregion
}
