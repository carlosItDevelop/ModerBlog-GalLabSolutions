using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernBlog.Models;
using ModernBlog.Services;
using Microsoft.EntityFrameworkCore;

namespace ModernBlog.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PostsController : Controller
{
    private readonly IPostService _postService;
    private readonly IImageService _imageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PostsController(IPostService postService, IImageService imageService, UserManager<ApplicationUser> userManager)
    {
        _postService = postService;
        _imageService = imageService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var posts = await _postService.GetAllPostsAsync(page, 10);
        return View(posts);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Post());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Post post, IFormFile? featuredImage, List<Guid> selectedTags)
    {
        Console.WriteLine("🔄 Tentativa de criar post...");
        Console.WriteLine($"📝 Título: {post.Title}");
        Console.WriteLine($"📝 Categoria: {post.CategoryId}");
        Console.WriteLine($"📝 Conteúdo (primeiros 100 chars): {post.Content?.Substring(0, Math.Min(100, post.Content?.Length ?? 0))}");
        Console.WriteLine($"📝 ModelState válido: {ModelState.IsValid}");

        if (!ModelState.IsValid)
        {
            Console.WriteLine("❌ ModelState inválido:");
            foreach (var error in ModelState)
            {
                if (error.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Campo: {error.Key}");
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"  Erro: {err.ErrorMessage}");
                    }
                }
            }
            await PopulateDropdowns();
            return View(post);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("❌ Usuário não encontrado");
                ModelState.AddModelError("", "User not found");
                await PopulateDropdowns();
                return View(post);
            }

            Console.WriteLine($"👤 Usuário encontrado: {user.Email}");
            post.AuthorId = user.Id;

            // Handle image upload
            if (featuredImage != null && _imageService.ValidateImage(featuredImage))
            {
                try
                {
                    Console.WriteLine("🖼️ Salvando imagem...");
                    post.FeaturedImageUrl = await _imageService.SaveImageAsync(featuredImage, "posts");
                    Console.WriteLine($"✅ Imagem salva: {post.FeaturedImageUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao salvar imagem: {ex.Message}");
                    ModelState.AddModelError("FeaturedImage", ex.Message);
                    await PopulateDropdowns();
                    return View(post);
                }
            }

            Console.WriteLine("💾 Criando post no banco...");
            var createdPost = await _postService.CreatePostAsync(post);
            Console.WriteLine($"✅ Post criado com ID: {createdPost.Id}");

            // Handle tags
            if (selectedTags?.Any() == true)
            {
                Console.WriteLine($"🏷️ Tags selecionadas: {selectedTags.Count}");
                // This would need additional logic to handle PostTag relationships
                // For now, we'll skip this part as it requires more complex implementation
            }

            TempData["Success"] = "Post criado com sucesso!";
            Console.WriteLine("✅ Redirecionando para Index...");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao criar post: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            ModelState.AddModelError("", $"Erro ao criar post: {ex.Message}");
            await PopulateDropdowns();
            return View(post);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        await PopulateDropdowns();
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Post post, IFormFile? featuredImage)
    {
        if (id != post.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Get existing post to preserve certain fields
                var existingPost = await _postService.GetPostByIdAsync(id);
                if (existingPost == null)
                    return NotFound();

                // Update fields
                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                existingPost.Summary = post.Summary;
                existingPost.CategoryId = post.CategoryId;
                existingPost.IsPublished = post.IsPublished;
                existingPost.IsFeatured = post.IsFeatured;
                existingPost.UpdatedAt = DateTime.UtcNow;

                // Handle image upload
                if (featuredImage != null && _imageService.ValidateImage(featuredImage))
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingPost.FeaturedImageUrl))
                    {
                        await _imageService.DeleteImageAsync(existingPost.FeaturedImageUrl);
                    }

                    var imagePath = await _imageService.SaveImageAsync(featuredImage, "posts");
                    existingPost.FeaturedImageUrl = imagePath;
                }

                // Update published date if publishing for first time
                if (post.IsPublished && existingPost.PublishedAt == null)
                {
                    existingPost.PublishedAt = DateTime.UtcNow;
                }

                await _postService.UpdatePostAsync(existingPost);
                TempData["Success"] = "Post atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PostExists(post.Id))
                    return NotFound();
                throw;
            }
        }

        await PopulateDropdowns();
        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _postService.DeletePostAsync(id);
            TempData["Success"] = "Post deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns()
    {
        var categories = await _postService.GetCategoriesAsync();
        var tags = await _postService.GetTagsAsync();

        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Tags = new SelectList(tags, "Id", "Name");
    }

    private string GenerateSlug(string title)
    {
        // Remove diacritics
        string normalizedString = title.Normalize(System.Text.NormalizationForm.FormD);
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

        foreach (char c in normalizedString)
        {
            System.Globalization.UnicodeCategory unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Remove accents and replace spaces with hyphens
        string str = stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLower();
        str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-").Trim();
        str = System.Text.RegularExpressions.Regex.Replace(str, @"-+", "-");
        return str;
    }

    private async Task<bool> PostExists(Guid id)
    {
        return await _postService.GetPostByIdAsync(id) != null;
    }
}