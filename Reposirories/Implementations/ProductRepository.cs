using BanHang.DTO.Product;
using BanHang.Models;
using BanHang.Reposirories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Reposirories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
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
            try
            {
                // Tạo product entity từ view model
                var product = new Product
                {
                    Name = productVM.Name,
                    Price = productVM.Price,
                    Description = productVM.Description,
                    CategoryId = productVM.CategoryId,
                    ImageUrl = productVM.ImageUrl
                };

                // Lưu product để lấy Id
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                // Lưu các hình ảnh bổ sung
                if (productVM.AdditionalImageUrls != null && productVM.AdditionalImageUrls.Any())
                {
                    foreach (var imageUrl in productVM.AdditionalImageUrls)
                    {
                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            Url = imageUrl
                        };
                        await _context.ProductImages.AddAsync(productImage);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm sản phẩm: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Xóa các hình ảnh phụ trước
            var productImages = await _context.ProductImages
                .Where(pi => pi.ProductId == id)
                .ToListAsync();

            if (productImages.Any())
            {
                _context.ProductImages.RemoveRange(productImages);
                await _context.SaveChangesAsync();
            }

            // Sau đó xóa sản phẩm
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
