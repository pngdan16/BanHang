using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BanHang.DTO.Product
{
    public class ProductViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(0.01, 1000000000, ErrorMessage = "Giá phải từ 0.01 đến 1,000,000,000")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình ảnh")]
        [Display(Name = "Hình ảnh chính")]
        public IFormFile ImageFile { get; set; }

        // Property này sẽ được sử dụng để lưu đường dẫn ảnh chính trong database
        public string ImageUrl { get; set; }
        
        [Display(Name = "Hình ảnh bổ sung")]
        public List<IFormFile> AdditionalImages { get; set; }
        
        // Danh sách đường dẫn của các hình ảnh bổ sung
        public List<string> AdditionalImageUrls { get; set; }
        
        public ProductViewModel()
        {
            AdditionalImages = new List<IFormFile>();
            AdditionalImageUrls = new List<string>();
        }
    }
}
