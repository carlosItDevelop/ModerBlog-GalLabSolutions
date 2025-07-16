
namespace ModernBlog.Services;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile imageFile, string subfolder = "");
    Task<bool> DeleteImageAsync(string imagePath);
    bool ValidateImage(IFormFile imageFile);
    Task<string> ResizeImageAsync(string imagePath, int width, int height);
}
