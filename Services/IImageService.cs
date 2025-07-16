
namespace ModernBlog.Services;

public interface IImageService
{
    Task<string?> SaveImageAsync(IFormFile file, string folder = "uploads");
    Task<bool> DeleteImageAsync(string imagePath);
    bool ValidateImage(IFormFile file);
    Task<string?> ResizeImageAsync(string imagePath, int width, int height);
}
