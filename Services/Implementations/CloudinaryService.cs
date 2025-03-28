using BanHang.Models.Settings;
using BanHang.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BanHang.Services.Implementations;
public class CloudinaryService : ICloudinaryService
{
  private readonly Cloudinary _cloudinary;
  private readonly ILogger<CloudinaryService> _logger;

  public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
  {
    _logger = logger;

    try
    {
      var cloudinarySettings = options.Value;

      if (string.IsNullOrEmpty(cloudinarySettings.CloudName) ||
          string.IsNullOrEmpty(cloudinarySettings.ApiKey) ||
          string.IsNullOrEmpty(cloudinarySettings.ApiSecret))
      {
        var error = "Thiếu cấu hình Cloudinary. Vui lòng kiểm tra appsettings.json";
        _logger.LogError(error);
        throw new ArgumentException(error);
      }

      _logger.LogInformation("Khởi tạo CloudinaryService với CloudName: {CloudName}", cloudinarySettings.CloudName);

      var account = new Account(cloudinarySettings.CloudName, cloudinarySettings.ApiKey, cloudinarySettings.ApiSecret);
      _cloudinary = new Cloudinary(account);

      _logger.LogInformation("Cloudinary service initialized successfully with cloud name: {CloudName}", cloudinarySettings.CloudName);
    }
    catch (Exception ex) when (ex is not InvalidOperationException)
    {
      _logger.LogError(ex, "Error initializing Cloudinary service");
      throw new InvalidOperationException("Failed to initialize Cloudinary service", ex);
    }
  }

  /// <summary>
  /// Tải lên một hình ảnh lên Cloudinary
  /// </summary>
  /// <param name="file">File hình ảnh</param>
  /// <returns>Kết quả tải lên hình ảnh</returns>
  public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
  {
    try
    {
      _logger.LogInformation("Uploading image to Cloudinary: {FileName}, Size: {Size}KB", file?.FileName, file?.Length / 1024);

      if (file == null || file.Length == 0)
      {
        _logger.LogWarning("Tệp hình ảnh trống hoặc không tồn tại");
        return null;
      }

      // Validate file type
      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".gif" && extension != ".webp")
      {
        _logger.LogError("Upload failed: Invalid file type: {FileType}", extension);
        throw new ArgumentException($"Định dạng file không hợp lệ. Chỉ chấp nhận: jpg, jpeg, png, gif, webp. Định dạng hiện tại: {extension}");
      }

      await using var stream = file.OpenReadStream();

      var uploadParams = new ImageUploadParams
      {
        File = new FileDescription(file.FileName, stream),
        Transformation = new Transformation().Quality("auto").FetchFormat("auto")
      };

      _logger.LogDebug("Sending upload request to Cloudinary for file: {FileName}", file.FileName);
      var uploadResult = await _cloudinary.UploadAsync(uploadParams);

      if (uploadResult.Error != null)
      {
        _logger.LogError("Lỗi Cloudinary: {Error}", uploadResult.Error.Message);
        return null;
      }

      _logger.LogInformation("Đã tải hình ảnh lên thành công, URL: {Url}", uploadResult.SecureUrl.AbsoluteUri);
      return uploadResult;
    }
    catch (Exception ex) when (ex is not ArgumentException)
    {
      _logger.LogError(ex, "Lỗi khi tải hình ảnh lên Cloudinary: {Error}", ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Xóa một phương tiện từ Cloudinary
  /// </summary>
  /// <param name="publicId">Public ID của phương tiện cần xóa</param>
  /// <returns>Kết quả xóa</returns>
  public async Task<DeletionResult> DeleteMediaAsync(string publicId)
  {
    try
    {
      _logger.LogInformation("Deleting media from Cloudinary: {PublicId}", publicId);

      if (string.IsNullOrEmpty(publicId))
      {
        _logger.LogWarning("PublicId trống, không thể xóa hình ảnh");
        throw new ArgumentException("Public ID cannot be null or empty", nameof(publicId));
      }

      // Extract only the public ID without folder path if needed
      string extractedPublicId = publicId;
      if (publicId.Contains("/"))
      {
        extractedPublicId = publicId.Substring(publicId.LastIndexOf("/") + 1);
        _logger.LogDebug("Extracted public ID from path: {ExtractedId}", extractedPublicId);
      }

      var deletionParams = new DeletionParams(extractedPublicId);
      _logger.LogDebug("Sending deletion request to Cloudinary for ID: {PublicId}", extractedPublicId);
      var result = await _cloudinary.DestroyAsync(deletionParams);

      if (result.Result != "ok")
      {
        _logger.LogError("Deletion from Cloudinary failed: {Error}", result.Result);
        throw new Exception($"Media deletion failed: {result.Result}");
      }

      _logger.LogInformation("Media deleted successfully: {PublicId}", publicId);
      return result;
    }
    catch (Exception ex) when (ex is not ArgumentException)
    {
      _logger.LogError(ex, "Lỗi khi xóa hình ảnh từ Cloudinary: {Error}", ex.Message);
      throw new Exception($"Unexpected error deleting media: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Tạo URL của hình ảnh với các biến đổi tùy chỉnh
  /// </summary>
  /// <param name="publicId">Public ID của hình ảnh trên Cloudinary</param>
  /// <param name="transformations">Dictionary chứa các tham số biến đổi</param>
  /// <returns>URL đầy đủ của hình ảnh đã biến đổi</returns>
  public async Task<string> GetImageUrl(string publicId, Dictionary<string, string> transformations = null)
  {
    try
    {
      _logger.LogDebug("Generating image URL for publicId: {PublicId}", publicId);

      if (string.IsNullOrEmpty(publicId))
      {
        _logger.LogWarning("PublicId trống, không thể lấy URL hình ảnh");
        return null;
      }

      // Tạo đối tượng transformation
      var transformation = new Transformation();

      // Thêm các tham số biến đổi vào transformation nếu có
      if (transformations != null && transformations.Count > 0)
      {
        _logger.LogDebug("Applying {Count} transformations", transformations.Count);
        foreach (var kvp in transformations)
        {
          // Xử lý một số tham số phổ biến đặc biệt
          switch (kvp.Key.ToLower())
          {
            case "width":
              if (int.TryParse(kvp.Value, out int width))
                transformation.Width(width);
              break;
            case "height":
              if (int.TryParse(kvp.Value, out int height))
                transformation.Height(height);
              break;
            case "crop":
              transformation.Crop(kvp.Value);
              break;
            case "gravity":
              transformation.Gravity(kvp.Value);
              break;
            case "quality":
              transformation.Quality(kvp.Value);
              break;
            case "effect":
              transformation.Effect(kvp.Value);
              break;
            case "radius":
              transformation.Radius(kvp.Value);
              break;
            default:
              // Thêm tham số tùy chỉnh khác
              transformation.Add(kvp.Key, kvp.Value);
              break;
          }
        }
      }
      else
      {
        _logger.LogDebug("Using default transformation settings");
        // Nếu không có transformations, sử dụng cấu hình mặc định
        transformation
          .Width(1000)
          .Crop("limit")
          .Quality("auto");
      }

      // Tạo URL
      var url = _cloudinary.Api.UrlImgUp
        .Transform(transformation)
        .BuildUrl(publicId);

      _logger.LogDebug("Generated Cloudinary URL: {Url}", url);

      return await Task.FromResult(url);
    }
    catch (Exception ex) when (ex is not ArgumentException)
    {
      _logger.LogError(ex, "Lỗi khi lấy URL hình ảnh từ Cloudinary: {Error}", ex.Message);
      return null;
    }
  }
}



