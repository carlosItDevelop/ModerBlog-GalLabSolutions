
using Microsoft.EntityFrameworkCore;
using ModernBlog.Data;
using ModernBlog.Models;
using Microsoft.AspNetCore.Identity;

namespace ModernBlog;

public static class DatabaseTester
{
    public static async Task TestConnection(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        Console.WriteLine("🔍 === TESTE DE CONEXÃO COM BANCO ===");
        
        try
        {
            // Testar conexão
            await context.Database.OpenConnectionAsync();
            Console.WriteLine("✅ Conexão com banco estabelecida!");
            await context.Database.CloseConnectionAsync();
            
            // Contar usuários
            var userCount = await context.Users.CountAsync();
            Console.WriteLine($"👥 Total de usuários: {userCount}");
            
            // Listar todos os usuários
            var users = await context.Users.ToListAsync();
            foreach (var user in users)
            {
                Console.WriteLine($"📧 Usuário: {user.Email} | Nome: {user.FirstName} {user.LastName} | ID: {user.Id}");
            }
            
            // Testar criação de usuário
            Console.WriteLine("\n🧪 === TESTE DE CRIAÇÃO DE USUÁRIO ===");
            var testUser = new ApplicationUser
            {
                UserName = "teste@teste.com",
                Email = "teste@teste.com",
                FirstName = "Teste",
                LastName = "Usuario",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(testUser, "Teste123!");
            if (result.Succeeded)
            {
                Console.WriteLine("✅ Usuário de teste criado com sucesso!");
                
                // Remover usuário de teste
                await userManager.DeleteAsync(testUser);
                Console.WriteLine("🗑️ Usuário de teste removido!");
            }
            else
            {
                Console.WriteLine("❌ Falha ao criar usuário de teste:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error.Code}: {error.Description}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro na conexão: {ex.Message}");
            Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");
        }
    }
}
