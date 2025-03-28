using BanHang.DTO.Product;
using BanHang.Models;

namespace BanHang.Reposirories.Interfaces
{
    public interface IProductRepository
    {
        //Lấy danh sách sản phẩm
        Task<IEnumerable<Product>> GetAllAsync();
        //Lấy sản phẩm theo id
        Task<Product> GetByIdAsync(int id);
        //Thêm sản phẩm
        Task AddAsync(ProductViewModel product);
        //Cập nhật sản phẩm
        Task UpdateAsync(Product product);
        //Xóa sản phẩm
        Task DeleteAsync(int id);
        //Xóa một hình ảnh bổ sung của sản phẩm
        Task<bool> DeleteProductImageAsync(int productId, int imageId);
    }
}
