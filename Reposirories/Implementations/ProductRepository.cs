using BanHang.DTO.Product;
using BanHang.Models;
using BanHang.Reposirories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace BanHang.Reposirories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(ProductViewModel productVM)
        {
            _logger.LogInformation("Bắt đầu thêm sản phẩm vào database: {ProductName}", productVM.Name);

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra nếu CategoryId không tồn tại
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
                if (!categoryExists)
                {
                    throw new Exception($"Danh mục với ID {productVM.CategoryId} không tồn tại");
                }

                // Kiểm tra ImageUrl
                if (string.IsNullOrEmpty(productVM.ImageUrl))
                {
                    throw new Exception("URL hình ảnh chính không được để trống");
                }

                // Tạo product entity từ view model
                var product = new Product
                {
                    Name = productVM.Name,
                    Price = productVM.Price,
                    Description = productVM.Description,
                    CategoryId = productVM.CategoryId,
                    ImageUrl = productVM.ImageUrl
                };

                _logger.LogInformation("Lưu thông tin sản phẩm chính: {ProductName}", product.Name);

                // Lưu product để lấy Id
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                // Lưu các hình ảnh bổ sung
                if (productVM.AdditionalImageUrls != null && productVM.AdditionalImageUrls.Any())
                {
                    _logger.LogInformation("Lưu {Count} hình ảnh bổ sung cho sản phẩm {ProductName}",
                        productVM.AdditionalImageUrls.Count, product.Name);

                    foreach (var imageUrl in productVM.AdditionalImageUrls)
                    {
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var productImage = new ProductImage
                            {
                                ProductId = product.Id,
                                Url = imageUrl
                            };
                            await _context.ProductImages.AddAsync(productImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                // Commit transaction nếu tất cả các thao tác thành công
                await transaction.CommitAsync();
                _logger.LogInformation("Đã thêm sản phẩm thành công: {ProductId} - {ProductName}",
                    product.Id, product.Name);
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm: {Error}", ex.Message);
                throw new Exception($"Lỗi khi thêm sản phẩm: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Product product)
        {
            _logger.LogInformation("Bắt đầu cập nhật sản phẩm: {ProductId} - {ProductName}", product.Id, product.Name);

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra sản phẩm tồn tại
                var existingProduct = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingProduct == null)
                {
                    _logger.LogError("Không tìm thấy sản phẩm ID: {ProductId} để cập nhật", product.Id);
                    throw new Exception($"Không tìm thấy sản phẩm ID: {product.Id} để cập nhật");
                }

                // Kiểm tra danh mục tồn tại
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
                if (!categoryExists)
                {
                    _logger.LogError("Danh mục không tồn tại: {CategoryId}", product.CategoryId);
                    throw new Exception($"Danh mục với ID {product.CategoryId} không tồn tại");
                }

                // Cập nhật thông tin cơ bản
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // Cập nhật hình ảnh chính nếu có thay đổi
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != existingProduct.ImageUrl)
                {
                    _logger.LogInformation("Cập nhật hình ảnh chính cho sản phẩm {ProductId}", product.Id);
                    existingProduct.ImageUrl = product.ImageUrl;
                }

                // Xử lý hình ảnh bổ sung
                if (product.Images != null && product.Images.Any())
                {
                    _logger.LogInformation("Xử lý hình ảnh bổ sung cho sản phẩm {ProductId}", product.Id);

                    // Thêm hình ảnh mới nếu có
                    foreach (var newImage in product.Images.Where(i => i.Id == 0))
                    {
                        _logger.LogInformation("Thêm hình ảnh bổ sung mới: {ImageUrl}", newImage.Url);
                        existingProduct.Images.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            Url = newImage.Url
                        });
                    }
                }

                // Cập nhật sản phẩm
                _context.Update(existingProduct);
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Cập nhật sản phẩm thành công: {ProductId} - {ProductName}",
                    product.Id, product.Name);
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm: {Error}", ex.Message);
                throw new Exception($"Lỗi khi cập nhật sản phẩm: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteProductImageAsync(int productId, int imageId)
        {
            _logger.LogInformation("Xóa hình ảnh {ImageId} của sản phẩm {ProductId}", imageId, productId);

            try
            {
                // Tìm hình ảnh bổ sung cần xóa
                var productImage = await _context.ProductImages
                    .FirstOrDefaultAsync(pi => pi.Id == imageId && pi.ProductId == productId);

                if (productImage == null)
                {
                    _logger.LogWarning("Không tìm thấy hình ảnh {ImageId} của sản phẩm {ProductId} để xóa",
                        imageId, productId);
                    return false;
                }

                // Xóa hình ảnh
                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa hình ảnh {ImageId} của sản phẩm {ProductId}", imageId, productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hình ảnh {ImageId} của sản phẩm {ProductId}: {Error}",
                    imageId, productId, ex.Message);
                return false;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xóa sản phẩm: {ProductId}", id);

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Tìm sản phẩm bao gồm các hình ảnh
                var product = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm {ProductId} để xóa", id);
                    throw new Exception($"Không tìm thấy sản phẩm có ID {id} để xóa");
                }

                // Xóa các hình ảnh phụ trước
                if (product.Images != null && product.Images.Any())
                {
                    _logger.LogInformation("Xóa {Count} hình ảnh bổ sung của sản phẩm {ProductId}",
                        product.Images.Count, id);

                    _context.ProductImages.RemoveRange(product.Images);
                    await _context.SaveChangesAsync();
                }

                // Sau đó xóa sản phẩm
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                // Commit transaction nếu tất cả các thao tác thành công
                await transaction.CommitAsync();
                _logger.LogInformation("Đã xóa sản phẩm thành công: {ProductId} - {ProductName}",
                    id, product.Name);
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm {ProductId}: {Error}", id, ex.Message);
                throw new Exception($"Lỗi khi xóa sản phẩm: {ex.Message}", ex);
            }
        }
    }
}
