using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModernBlog.Services;

namespace ModernBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IPostService _postService;

        public DashboardController(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            // Verificar se o usuário está autenticado e é admin
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Challenge(); // Redireciona para login
            }

            if (!User.IsInRole("Admin"))
            {
                return Forbid(); // Acesso negado
            }

            var totalPosts = await _postService.GetTotalPostsAsync();
            var publishedPosts = await _postService.GetPublishedPostsCountAsync();
            var recentPosts = await _postService.GetRecentPostsAsync(5);

            ViewBag.TotalPosts = totalPosts;
            ViewBag.PublishedPosts = publishedPosts;
            ViewBag.RecentPosts = recentPosts;

            return View();
        }
    }
}