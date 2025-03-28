using BanHang.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Controllers;
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(
        ICloudinaryService cloudinaryService,
        ILogger<MediaController> logger)
    {
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    [HttpPost("upload/image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please provide a valid file");

            // Validate file type
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !IsValidImageExtension(extension))
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed.");

            // Validate file size (e.g., limit to 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("File size exceeds the limit (10MB)");

            var result = await _cloudinaryService.UploadImageAsync(file);

            var response = new
            {
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId,
                Format = result.Format,
                Width = result.Width,
                Height = result.Height,
                Size = result.Bytes,
                CreatedAt = result.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }



    [HttpDelete("{publicId}")]
    public async Task<IActionResult> DeleteMedia(string publicId)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
                return BadRequest("Public ID is required");

            var result = await _cloudinaryService.DeleteMediaAsync(publicId);

            return Ok(new { Message = "Media deleted successfully", Result = result.Result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media with public ID: {PublicId}", publicId);
            return StatusCode(500, "An error occurred while deleting the media");
        }
    }

    [HttpGet("image/{publicId}")]
    public async Task<IActionResult> GetImageWithTransformations(string publicId, [FromQuery] string width, [FromQuery] string height, [FromQuery] string crop)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
                return BadRequest("Public ID is required");

            var transformations = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(width))
                transformations.Add("width", width);

            if (!string.IsNullOrEmpty(height))
                transformations.Add("height", height);

            if (!string.IsNullOrEmpty(crop))
                transformations.Add("crop", crop);

            //get url from upload image 
            var url = await _cloudinaryService.GetImageUrl(publicId, transformations);

            return Ok(new { Url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image URL for public ID: {PublicId}", publicId);
            return StatusCode(500, "An error occurred while generating the image URL");
        }
    }

    private bool IsValidImageExtension(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => true,
            _ => false
        };
    }

    private bool IsValidVideoExtension(string extension)
    {
        return extension switch
        {
            ".mp4" or ".mov" or ".avi" or ".wmv" or ".webm" => true,
            _ => false
        };
    }
}