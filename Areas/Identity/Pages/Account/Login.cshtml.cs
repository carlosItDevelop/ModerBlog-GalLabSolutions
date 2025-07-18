
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ModernBlog.Models;

namespace ModernBlog.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public string ReturnUrl { get; set; } = string.Empty;

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "O campo Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "O campo Senha é obrigatório.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Lembrar de mim")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("🔍 LOGIN: Iniciando processo de login");
                _logger.LogInformation($"📧 LOGIN: Email recebido: {Input.Email}");
                _logger.LogInformation($"🔙 LOGIN: ReturnUrl: {returnUrl}");
                
                returnUrl ??= Url.Content("~/");

                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
                _logger.LogInformation($"✅ LOGIN: ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("DEBUG: ModelState is NOT valid. Errors:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"- {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                        _logger.LogError($"❌ LOGIN: ModelState Error - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    _logger.LogError("❌ LOGIN: Retornando página devido a ModelState inválido");
                    return Page();
                }
                
                _logger.LogInformation($"🔐 LOGIN: Tentando fazer login com {Input.Email}");
                
                // Verificar se o usuário existe ANTES de tentar login
                ApplicationUser? existingUser = null;
                try
                {
                    existingUser = await _userManager.FindByEmailAsync(Input.Email);
                    _logger.LogInformation($"👤 LOGIN: Usuário encontrado: {existingUser?.Email ?? "NÃO ENCONTRADO"}");
                    
                    if (existingUser != null)
                    {
                        _logger.LogInformation($"📋 LOGIN: Detalhes do usuário - ID: {existingUser.Id}, EmailConfirmed: {existingUser.EmailConfirmed}");
                        
                        // Verificar se a senha está correta ANTES do SignIn
                        var passwordCheck = await _userManager.CheckPasswordAsync(existingUser, Input.Password);
                        _logger.LogInformation($"🔑 LOGIN: Verificação de senha: {passwordCheck}");
                        
                        // Verificar roles do usuário
                        var userRoles = await _userManager.GetRolesAsync(existingUser);
                        _logger.LogInformation($"🏷️ LOGIN: Roles do usuário: {string.Join(", ", userRoles)}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ LOGIN: Erro ao buscar usuário: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Erro interno do servidor. Tente novamente.");
                    return Page();
                }
                
                if (existingUser == null)
                {
                    _logger.LogWarning("⚠️ LOGIN: Usuário não encontrado");
                    ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                    return Page();
                }
                
                // Tentar fazer login
                Microsoft.AspNetCore.Identity.SignInResult result;
                try
                {
                    _logger.LogInformation("🚀 LOGIN: Executando PasswordSignInAsync...");
                    result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                    _logger.LogInformation($"🎯 LOGIN: Resultado - Succeeded: {result.Succeeded}, RequiresTwoFactor: {result.RequiresTwoFactor}, IsLockedOut: {result.IsLockedOut}, IsNotAllowed: {result.IsNotAllowed}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ LOGIN: Erro durante SignIn: {ex.Message}");
                    _logger.LogError($"❌ LOGIN: Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError(string.Empty, "Erro interno durante login. Tente novamente.");
                    return Page();
                }
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ LOGIN: Login realizado com sucesso!");
                    
                    // Verificar se é admin e redirecionar para área administrativa
                    if (await _userManager.IsInRoleAsync(existingUser, "Admin"))
                    {
                        _logger.LogInformation("🔑 LOGIN: Admin detectado, redirecionando para dashboard");
                        return LocalRedirect(returnUrl.Contains("/Admin") ? returnUrl : "/Admin/Dashboard");
                    }
                    
                    // Se não é admin, ir para home ou returnUrl
                    _logger.LogInformation($"🏠 LOGIN: Usuário regular, redirecionando para: {returnUrl}");
                    return LocalRedirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("🔒 LOGIN: Conta bloqueada");
                    return RedirectToPage("./Lockout");
                }
                else if (result.IsNotAllowed)
                {
                    _logger.LogWarning("🚫 LOGIN: Login não permitido");
                    ModelState.AddModelError(string.Empty, "Login não permitido. Confirme seu email.");
                    return Page();
                }
                else if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("📱 LOGIN: Requer autenticação de dois fatores");
                    // Implementar 2FA se necessário
                    ModelState.AddModelError(string.Empty, "Autenticação de dois fatores necessária.");
                    return Page();
                }
                else
                {
                    _logger.LogWarning("❌ LOGIN: Falha na autenticação");
                    ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"💥 LOGIN: Erro geral: {ex.Message}");
                _logger.LogError($"💥 LOGIN: Stack trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "Erro interno do servidor. Tente novamente.");
                return Page();
            }
        }
    }
}
