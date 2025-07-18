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

            try
            {
                var totalPosts = await _postService.GetTotalPostsAsync();
                var publishedPosts = await _postService.GetPublishedPostsCountAsync();
                var recentPosts = await _postService.GetRecentPostsAsync(5);

                ViewBag.TotalPosts = totalPosts;
                ViewBag.PublishedPosts = publishedPosts;
                ViewBag.RecentPosts = recentPosts.ToList();
            }
            catch (Exception ex)
            {
                // Em caso de erro, usar valores padrão
                ViewBag.TotalPosts = 0;
                ViewBag.PublishedPosts = 0;
                ViewBag.RecentPosts = new List<ModernBlog.Models.Post>();
                
                // Log do erro (opcional)
                Console.WriteLine($"Erro ao carregar dashboard: {ex.Message}");
            }

            return View();
        }
    }
}