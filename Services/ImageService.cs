using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ModernBlog.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ImageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<string> SaveImageAsync(IFormFile imageFile, string subfolder = "")
    {
        if (!ValidateImage(imageFile))
            throw new ArgumentException("Invalid image file");

        var uploadPath = Path.Combine(_environment.WebRootPath, "images", "uploads", subfolder);
        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var image = await Image.LoadAsync(imageFile.OpenReadStream()))
        {
            // Resize to featured image size (800x450) if larger
            if (image.Width > 800 || image.Height > 450)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 450),
                    Mode = ResizeMode.Crop
                }));
            }

            await image.SaveAsJpegAsync(filePath);
        }

        return $"/images/uploads/{subfolder}/{fileName}".Replace("//", "/");
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
            // Log error in production
        }
        return false;
    }

    public bool ValidateImage(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return false;

        var maxSize = _configuration.GetValue<int>("Blog:MaxImageSize", 2097152); // 2MB default
        if (imageFile.Length > maxSize)
            return false;

        var allowedTypes = _configuration.GetSection("Blog:AllowedImageTypes").Get<string[]>() 
            ?? new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

        return allowedTypes.Contains(imageFile.ContentType.ToLower());
    }

    public async Task<string> ResizeImageAsync(string imagePath, int width, int height)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Image not found");

        using (var image = await Image.LoadAsync(fullPath))
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop
            }));

            var newFileName = $"resized_{width}x{height}_{Path.GetFileName(fullPath)}";
            var newPath = Path.Combine(Path.GetDirectoryName(fullPath)!, newFileName);

            await image.SaveAsJpegAsync(newPath);

            return imagePath.Replace(Path.GetFileName(imagePath), newFileName);
        }
    }
}