using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModernBlog.Services;
using ModernBlog.Models;

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

            try
            {
                // Add retry delay for dashboard loads
                await Task.Delay(100); // Small delay to ensure connection is ready

                ViewBag.TotalPosts = await _postService.GetTotalPostsAsync();
                ViewBag.PublishedPosts = await _postService.GetPublishedPostsCountAsync();
                ViewBag.RecentPosts = await _postService.GetRecentPostsAsync();
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro ao carregar dashboard: {ex.Message}");

                // Wait and try one more time
                try
                {
                    await Task.Delay(1000);
                    ViewBag.TotalPosts = await _postService.GetTotalPostsAsync();
                    ViewBag.PublishedPosts = await _postService.GetPublishedPostsCountAsync();
                    ViewBag.RecentPosts = await _postService.GetRecentPostsAsync();
                    return View();
                }
                catch
                {
                    // Final fallback data
                    ViewBag.TotalPosts = 0;
                    ViewBag.PublishedPosts = 0;
                    ViewBag.RecentPosts = new List<Post>();
                    TempData["Warning"] = "Alguns dados podem estar desatualizados devido a problemas de conexão.";
                    return View();
                }
            }
        }
    }
}