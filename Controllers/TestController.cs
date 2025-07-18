
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModernBlog.Models;
using Microsoft.EntityFrameworkCore;
using ModernBlog.Data;

namespace ModernBlog.Controllers;

public class TestController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TestController> _logger;

    public TestController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        ILogger<TestController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet("/test/database")]
    public async Task<IActionResult> TestDatabase()
    {
        try
        {
            _logger.LogInformation("🔍 TEST: Iniciando teste de banco de dados");
            
            // Testar conexão
            await _context.Database.OpenConnectionAsync();
            _logger.LogInformation("✅ TEST: Conexão estabelecida");
            await _context.Database.CloseConnectionAsync();
            
            // Contar usuários
            var userCount = await _context.Users.CountAsync();
            _logger.LogInformation($"👥 TEST: Total de usuários: {userCount}");
            
            // Buscar usuário admin
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@modernblog.com");
            
            var result = new
            {
                DatabaseConnection = "OK",
                UserCount = userCount,
                AdminUser = admin != null ? new
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    EmailConfirmed = admin.EmailConfirmed,
                    PasswordHash = admin.PasswordHash?.Substring(0, 20) + "..."
                } : null
            };
            
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ TEST: Erro: {ex.Message}");
            return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpPost("/test/login")]
    public async Task<IActionResult> TestLogin(string email = "admin@modernblog.com", string password = "Admin123!")
    {
        try
        {
            _logger.LogInformation($"🧪 TEST: Simulando login - {email}");
            
            // Buscar usuário
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { Success = false, Message = "Usuário não encontrado" });
            }
            
            // Verificar senha
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                return Json(new { Success = false, Message = "Senha inválida" });
            }
            
            // Obter roles
            var roles = await _userManager.GetRolesAsync(user);
            
            // Tentar fazer login
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            
            return Json(new
            {
                Success = result.Succeeded,
                User = new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.EmailConfirmed
                },
                Roles = roles,
                SignInResult = new
                {
                    result.Succeeded,
                    result.IsLockedOut,
                    result.IsNotAllowed,
                    result.RequiresTwoFactor
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ TEST: Erro no teste de login: {ex.Message}");
            return Json(new { Success = false, Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }
}
