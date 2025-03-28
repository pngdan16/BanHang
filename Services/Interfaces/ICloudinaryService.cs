using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BanHang.Services.Interfaces;
public interface ICloudinaryService
{
    Task<ImageUploadResult> UploadImageAsync(IFormFile image);
    Task<DeletionResult> DeleteMediaAsync(string publicId);
    //string GetImageUrl(string publicId);
   /// <summary>
    /// Tạo URL của hình ảnh với các biến đổi tùy chỉnh
    /// </summary>
    /// <param name="publicId">Public ID của hình ảnh trên Cloudinary</param>
    /// <param name="transformations">Dictionary chứa các tham số biến đổi</param>
    /// <returns>URL đầy đủ của hình ảnh đã biến đổi</returns>
    Task<string> GetImageUrl(string publicId, Dictionary<string, string> transformations = null);
}


