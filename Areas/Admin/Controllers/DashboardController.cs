
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernBlog.Data;
using Microsoft.EntityFrameworkCore;

namespace ModernBlog.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalPosts = await _context.Posts.CountAsync();
        ViewBag.PublishedPosts = await _context.Posts.CountAsync(p => p.IsPublished);
        ViewBag.DraftPosts = await _context.Posts.CountAsync(p => !p.IsPublished);
        ViewBag.TotalCategories = await _context.Categories.CountAsync();
        ViewBag.TotalTags = await _context.Tags.CountAsync();
        ViewBag.TotalComments = await _context.Comments.CountAsync();
        ViewBag.PendingComments = await _context.Comments.CountAsync(c => !c.IsApproved);
        ViewBag.TotalUsers = await _context.Users.CountAsync();

        var recentPosts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(recentPosts);
    }
}
