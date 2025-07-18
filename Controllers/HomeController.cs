
using Microsoft.AspNetCore.Mvc;
using ModernBlog.Data;
using ModernBlog.Services;
using Microsoft.EntityFrameworkCore;
using ModernBlog.Models;
using Microsoft.AspNetCore.Identity;

namespace ModernBlog.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(IPostService postService, UserManager<ApplicationUser> userManager)
    {
        _postService = postService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var pageSize = 6;
        var posts = await _postService.GetPublishedPostsAsync(page, pageSize);
        var featuredPosts = await _postService.GetFeaturedPostsAsync(5);
        var recentPosts = await _postService.GetRecentPostsAsync(5);
        var categories = await _postService.GetCategoriesAsync();
        
        ViewBag.FeaturedPosts = featuredPosts;
        ViewBag.RecentPosts = recentPosts;
        ViewBag.Categories = categories;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPosts = await _postService.GetTotalPostsCountAsync();
        ViewBag.TotalPages = Math.Ceiling((double)ViewBag.TotalPosts / pageSize);

        return View(posts);
    }

    public async Task<IActionResult> Post(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        var post = await _postService.GetPostBySlugAsync(slug);
        if (post == null)
            return NotFound();

        // Increment view count
        await _postService.IncrementViewCountAsync(post.Id);

        // Get related posts
        var relatedPosts = await _postService.GetRelatedPostsAsync(post.Id, 3);
        ViewBag.RelatedPosts = relatedPosts;

        // Get sidebar data
        var recentPosts = await _postService.GetRecentPostsAsync(5);
        var categories = await _postService.GetCategoriesAsync();
        
        ViewBag.RecentPosts = recentPosts;
        ViewBag.Categories = categories;

        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(Guid postId)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Json(new { success = false, message = "Login required" });

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "User not found" });

        var liked = await _postService.ToggleLikeAsync(postId, user.Id);
        return Json(new { success = true, liked });
    }

    public async Task<IActionResult> Category(Guid id, int page = 1)
    {
        var posts = await _postService.GetPostsByCategoryAsync(id, 20);
        ViewBag.CurrentPage = page;
        return View("Index", posts);
    }
}
