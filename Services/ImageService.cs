
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ModernBlog.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly long _maxFileSize = 2 * 1024 * 1024; // 2MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveImageAsync(IFormFile file, string folder = "uploads")
    {
        if (!ValidateImage(file))
            return null;

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folder);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Auto-resize to featured image size (800x455)
        await ResizeImageAsync(filePath, 800, 455);

        return Path.Combine("images", folder, uniqueFileName).Replace("\\", "/");
    }

    public async Task<bool> DeleteImageAsync(string imagePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
        }
        catch
        {
            // Log error
        }
        return false;
    }

    public bool ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > _maxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;

        // Check if it's a valid image
        try
        {
            using var image = Image.Load(file.OpenReadStream());
            // Check aspect ratio (should be close to 16:9)
            var aspectRatio = (double)image.Width / image.Height;
            if (aspectRatio < 1.7 || aspectRatio > 1.8) // Allow some tolerance
                return false;

            // Check minimum dimensions
            if (image.Width < 535 || image.Height < 300)
                return false;

            // Check maximum dimensions
            if (image.Width > 1600 || image.Height > 900)
                return false;
        }
        catch
        {
            return false;
        }

        return true;
    }

    public async Task<string?> ResizeImageAsync(string imagePath, int width, int height)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
            
            using var image = await Image.LoadAsync(fullPath);
            image.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));
            await image.SaveAsync(fullPath);
            
            return imagePath;
        }
        catch
        {
            return null;
        }
    }
}
