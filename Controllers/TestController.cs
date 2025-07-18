
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

    [HttpGet("/test/login")]
    public IActionResult TestLoginPage()
    {
        return View("TestLogin");
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

    [HttpGet("/test/login-direct")]
    public async Task<IActionResult> TestLoginDirect()
    {
        var maxRetries = 3;
        var retryDelay = 1000; // 1 segundo
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation($"🧪 TEST: Teste direto de login - Tentativa {attempt}/{maxRetries}");
                
                var email = "admin@modernblog.com";
                var password = "Admin123!";
                
                // Aguardar um pouco se não é a primeira tentativa
                if (attempt > 1)
                {
                    await Task.Delay(retryDelay);
                    // Verificar se o contexto ainda está funcionando
                    await _context.Database.OpenConnectionAsync();
                    await _context.Database.CloseConnectionAsync();
                    _logger.LogInformation($"✅ TEST: Reconexão com banco bem-sucedida na tentativa {attempt}");
                }
                
                // Buscar usuário diretamente no contexto (mais confiável)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return Json(new { Success = false, Message = "Usuário não encontrado" });
                }
                
                _logger.LogInformation($"👤 TEST: Usuário encontrado: {user.Email}");
                
                // Verificar senha usando UserManager
                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    return Json(new { Success = false, Message = "Senha inválida" });
                }
                
                _logger.LogInformation("🔑 TEST: Senha válida");
                
                // Obter roles
                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation($"🏷️ TEST: Roles: {string.Join(", ", roles)}");
                
                // Tentar fazer login
                var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
                _logger.LogInformation($"🎯 TEST: SignIn Result - Succeeded: {result.Succeeded}");
                
                return Json(new
                {
                    Success = result.Succeeded,
                    Message = result.Succeeded ? "Login realizado com sucesso!" : "Falha no login",
                    Attempt = attempt,
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
                    },
                    RedirectUrl = result.Succeeded ? "/Admin/Dashboard" : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TEST: Erro na tentativa {attempt}: {ex.Message}");
                
                if (attempt == maxRetries)
                {
                    return Json(new { 
                        Success = false, 
                        Error = ex.Message, 
                        Attempts = maxRetries,
                        Message = "Falha após múltiplas tentativas - possível problema de conexão com banco"
                    });
                }
                
                _logger.LogInformation($"🔄 TEST: Tentando novamente em {retryDelay}ms...");
            }
        }
        
        return Json(new { Success = false, Message = "Erro inesperado" });
    }
}
