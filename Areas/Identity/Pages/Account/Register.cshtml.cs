
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ModernBlog.Models;

namespace ModernBlog.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = string.Empty;

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public class InputModel
        {
            [Required(ErrorMessage = "O campo Nome é obrigatório.")]
            [Display(Name = "Nome")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "O campo Sobrenome é obrigatório.")]
            [Display(Name = "Sobrenome")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "O campo Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "O campo Senha é obrigatório.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? string.Empty;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            _logger.LogInformation("🔍 REGISTRO: Iniciando processo de registro");
            _logger.LogInformation($"📧 REGISTRO: Email: {Input.Email}");
            _logger.LogInformation($"👤 REGISTRO: Nome: {Input.FirstName} {Input.LastName}");
            
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            _logger.LogInformation($"✅ REGISTRO: ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    _logger.LogError($"❌ REGISTRO: ModelState Error - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            if (ModelState.IsValid)
            {
                _logger.LogInformation("👤 REGISTRO: Criando novo usuário...");
                
                // Verificar se usuário já existe
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"⚠️ REGISTRO: Usuário {Input.Email} já existe!");
                    ModelState.AddModelError(string.Empty, "Este email já está cadastrado.");
                    return Page();
                }
                
                var user = new ApplicationUser 
                { 
                    UserName = Input.Email, 
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    CreatedAt = DateTime.UtcNow
                };
                
                _logger.LogInformation($"🔐 REGISTRO: Criando usuário {user.Email} (ID: {user.Id})");
                
                var result = await _userManager.CreateAsync(user, Input.Password);
                
                _logger.LogInformation($"🎯 REGISTRO: Resultado - Succeeded: {result.Succeeded}");
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"❌ REGISTRO: Erro - {error.Code}: {error.Description}");
                    }
                }
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"✅ REGISTRO: Usuário criado com sucesso: {user.Email} (ID: {user.Id})");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation($"🔐 REGISTRO: Login automático realizado");
                    _logger.LogInformation($"👤 REGISTRO: Usuário {user.Email} logado automaticamente após registro");
                    
                    // Ir direto para home após registro
                    _logger.LogInformation("🏠 REGISTRO: Redirecionando para home");
                    return Redirect("/");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
