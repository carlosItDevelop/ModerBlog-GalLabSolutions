
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModernBlog.Data;
using ModernBlog.Models;
using ModernBlog.Services;
using System.Security.Claims;

namespace ModernBlog.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Author")]
public class PostsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public PostsController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(posts);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var post = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (post == null) return NotFound();

        return View(post);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Content,Summary,IsPublished,IsFeatured,CategoryId")] Post post, 
        IFormFile? featuredImage, string[]? selectedTags)
    {
        if (ModelState.IsValid)
        {
            post.Id = Guid.NewGuid();
            post.AuthorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            if (post.IsPublished)
                post.PublishedAt = DateTime.UtcNow;

            // Handle featured image upload
            if (featuredImage != null && _imageService.ValidateImage(featuredImage))
            {
                post.FeaturedImageUrl = await _imageService.SaveImageAsync(featuredImage, "posts");
            }

            _context.Add(post);
            await _context.SaveChangesAsync();

            // Handle tags
            if (selectedTags != null && selectedTags.Length > 0)
            {
                await AddTagsToPostAsync(post.Id, selectedTags);
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdownsAsync();
        return View(post);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var post = await _context.Posts
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        await PopulateDropdownsAsync();
        ViewBag.SelectedTags = post.PostTags.Select(pt => pt.Tag.Name).ToArray();

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,Content,Summary,IsPublished,IsFeatured,CategoryId,FeaturedImageUrl")] Post post, 
        IFormFile? featuredImage, string[]? selectedTags)
    {
        if (id != post.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existingPost = await _context.Posts.FindAsync(id);
                if (existingPost == null) return NotFound();

                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                existingPost.Summary = post.Summary;
                existingPost.CategoryId = post.CategoryId;
                existingPost.UpdatedAt = DateTime.UtcNow;

                if (post.IsPublished && !existingPost.IsPublished)
                    existingPost.PublishedAt = DateTime.UtcNow;

                existingPost.IsPublished = post.IsPublished;
                existingPost.IsFeatured = post.IsFeatured;

                // Handle featured image upload
                if (featuredImage != null && _imageService.ValidateImage(featuredImage))
                {
                    // Delete old image
                    if (!string.IsNullOrEmpty(existingPost.FeaturedImageUrl))
                        await _imageService.DeleteImageAsync(existingPost.FeaturedImageUrl);

                    existingPost.FeaturedImageUrl = await _imageService.SaveImageAsync(featuredImage, "posts");
                }

                _context.Update(existingPost);

                // Update tags
                var existingTags = await _context.PostTags.Where(pt => pt.PostId == id).ToListAsync();
                _context.PostTags.RemoveRange(existingTags);

                if (selectedTags != null && selectedTags.Length > 0)
                {
                    await AddTagsToPostAsync(id, selectedTags);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(post.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdownsAsync();
        return View(post);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var post = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (post == null) return NotFound();

        return View(post);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            // Delete featured image
            if (!string.IsNullOrEmpty(post.FeaturedImageUrl))
                await _imageService.DeleteImageAsync(post.FeaturedImageUrl);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PostExists(Guid id)
    {
        return _context.Posts.Any(e => e.Id == id);
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
        ViewData["Tags"] = await _context.Tags.Select(t => t.Name).ToListAsync();
    }

    private async Task AddTagsToPostAsync(Guid postId, string[] tagNames)
    {
        foreach (var tagName in tagNames)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            var postTag = new PostTag { PostId = postId, TagId = tag.Id };
            _context.PostTags.Add(postTag);
        }
        await _context.SaveChangesAsync();
    }
}
