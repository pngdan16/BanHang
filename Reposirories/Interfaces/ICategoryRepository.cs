using BanHang.Models;

namespace BanHang.Reposirories.Interfaces
{
    public interface ICategoryRepository
    {
        //Lấy danh mục loại sản phẩm
        Task<IEnumerable<Category>> GetAllAsync();
        //Lấy danh mục loại sản phẩm theo id
        Task<Category> GetByIdAsync(int id);
        //Thêm danh mục loại sản phẩm
        Task AddAsync(Category category);
        //Cập nhật danh mục loại sản phẩm
        Task UpdateAsync(Category category);
        //Xóa danh mục loại sản phẩm
        Task DeleteAsync(int id);
    }
} 