using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernBlog.Models;
using ModernBlog.Services;

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
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                await PopulateDropdowns();
                return View(post);
            }

            post.AuthorId = user.Id;

            // Handle image upload
            if (featuredImage != null && _imageService.ValidateImage(featuredImage))
            {
                try
                {
                    post.FeaturedImageUrl = await _imageService.SaveImageAsync(featuredImage, "posts");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("FeaturedImage", ex.Message);
                    await PopulateDropdowns();
                    return View(post);
                }
            }

            var createdPost = await _postService.CreatePostAsync(post);

            // Handle tags
            if (selectedTags?.Any() == true)
            {
                // This would need additional logic to handle PostTag relationships
                // For now, we'll skip this part as it requires more complex implementation
            }

            TempData["Success"] = "Post created successfully!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns();
        return View(post);
    }

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
    public async Task<IActionResult> Edit(Guid id, Post post, IFormFile? featuredImage, List<Guid> selectedTags)
    {
        if (id != post.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Handle image upload
                if (featuredImage != null && _imageService.ValidateImage(featuredImage))
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(post.FeaturedImageUrl))
                    {
                        await _imageService.DeleteImageAsync(post.FeaturedImageUrl);
                    }

                    post.FeaturedImageUrl = await _imageService.SaveImageAsync(featuredImage, "posts");
                }

                await _postService.UpdatePostAsync(post);
                TempData["Success"] = "Post updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
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
}