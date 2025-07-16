using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernBlog.Services;

namespace ModernBlog.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DashboardController : Controller
{
    private readonly IPostService _postService;

    public DashboardController(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<IActionResult> Index()
    {
        var recentPosts = await _postService.GetAllPostsAsync(1, 10);
        var totalPosts = await _postService.GetTotalPostsCountAsync();

        ViewBag.TotalPosts = totalPosts;
        ViewBag.RecentPosts = recentPosts;

        return View();
    }
}