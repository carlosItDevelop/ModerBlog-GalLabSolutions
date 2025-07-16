
using Microsoft.AspNetCore.Mvc;
using ModernBlog.Data;
using ModernBlog.Services;
using Microsoft.EntityFrameworkCore;

namespace ModernBlog.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly ApplicationDbContext _context;

    public HomeController(IPostService postService, ApplicationDbContext context)
    {
        _postService = postService;
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 6;
        
        ViewBag.FeaturedPosts = await _postService.GetFeaturedPostsAsync(5);
        ViewBag.RecentPosts = await _postService.GetRecentPostsAsync(5);
        ViewBag.MostViewedPosts = await _postService.GetMostViewedPostsAsync(5);
        ViewBag.Categories = await _context.Categories.Include(c => c.Posts).ToListAsync();
        
        var posts = await _postService.GetPublishedPostsAsync(page, pageSize);
        var totalPosts = await _postService.GetTotalPublishedPostsCountAsync();
        
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalPosts / pageSize);
        
        return View(posts);
    }

    public async Task<IActionResult> Post(string slug)
    {
        var post = await _postService.GetPostBySlugAsync(slug);
        if (post == null)
            return NotFound();

        // Increment view count
        await _postService.IncrementViewCountAsync(post.Id);

        ViewBag.RelatedPosts = await _postService.GetRelatedPostsAsync(post.Id, 3);
        ViewBag.RecentPosts = await _postService.GetRecentPostsAsync(5);
        ViewBag.Categories = await _context.Categories.Include(c => c.Posts).ToListAsync();

        return View(post);
    }

    public async Task<IActionResult> Category(Guid id, int page = 1)
    {
        const int pageSize = 6;
        
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        var posts = await _postService.GetPostsByCategoryAsync(id, page, pageSize);
        var totalPosts = await _context.Posts.CountAsync(p => p.IsPublished && p.CategoryId == id);
        
        ViewBag.Category = category;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalPosts / pageSize);
        ViewBag.Categories = await _context.Categories.Include(c => c.Posts).ToListAsync();
        
        return View(posts);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(Guid postId)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        Guid? userId = User.Identity?.IsAuthenticated == true ? 
            Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "") : null;

        var liked = await _postService.ToggleLikeAsync(postId, ipAddress, userId);
        
        var post = await _context.Posts.FindAsync(postId);
        return Json(new { liked, likeCount = post?.LikeCount ?? 0 });
    }
}
