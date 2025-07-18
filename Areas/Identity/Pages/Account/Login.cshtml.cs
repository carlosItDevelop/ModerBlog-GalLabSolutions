
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
            _logger.LogInformation("🔍 LOGIN: Iniciando processo de login");
            _logger.LogInformation($"📧 LOGIN: Email recebido: {Input.Email}");
            _logger.LogInformation($"🔙 LOGIN: ReturnUrl: {returnUrl}");
            
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
            _logger.LogInformation($"✅ LOGIN: ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    _logger.LogError($"❌ LOGIN: ModelState Error - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            if (ModelState.IsValid)
            {
                _logger.LogInformation($"🔐 LOGIN: Tentando fazer login com {Input.Email}");
                
                // Verificar se o usuário existe
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                _logger.LogInformation($"👤 LOGIN: Usuário encontrado: {existingUser?.Email ?? "NÃO ENCONTRADO"}");
                
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                
                _logger.LogInformation($"🎯 LOGIN: Resultado - Succeeded: {result.Succeeded}, RequiresTwoFactor: {result.RequiresTwoFactor}, IsLockedOut: {result.IsLockedOut}, IsNotAllowed: {result.IsNotAllowed}");
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ LOGIN: Login realizado com sucesso!");
                    
                    // Debug: Log do usuário que fez login
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    _logger.LogInformation($"👤 LOGIN: Usuário logado: {user?.Email} (ID: {user?.Id})");
                    
                    // Verificar se é admin e redirecionar para área administrativa
                    if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        _logger.LogInformation("🔑 LOGIN: Admin detectado, redirecionando para dashboard");
                        return Redirect("/Admin/Dashboard");
                    }
                    
                    // Se não é admin, ir para home
                    _logger.LogInformation("🏠 LOGIN: Usuário regular, redirecionando para home");
                    return Redirect("/");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
