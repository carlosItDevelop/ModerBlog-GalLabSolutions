
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModernBlog.Models;

namespace ModernBlog.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.EnsureCreatedAsync();

        // Create roles
        string[] roleNames = { "Admin", "Author", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }
        }

        // Create admin user
        var adminUser = await userManager.FindByEmailAsync("admin@modernblog.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@modernblog.com",
                Email = "admin@modernblog.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Create default categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Tecnologia", Description = "Posts sobre tecnologia e inovação", Color = "#007bff" },
                new Category { Name = "Desenvolvimento", Description = "Artigos sobre desenvolvimento de software", Color = "#28a745" },
                new Category { Name = "Design", Description = "Posts sobre design e UX/UI", Color = "#fd7e14" },
                new Category { Name = "Negócios", Description = "Conteúdo sobre negócios e empreendedorismo", Color = "#6610f2" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Create default tags
        if (!await context.Tags.AnyAsync())
        {
            var tags = new[]
            {
                new Tag { Name = "ASP.NET Core" },
                new Tag { Name = "C#" },
                new Tag { Name = "Entity Framework" },
                new Tag { Name = "Bootstrap" },
                new Tag { Name = "JavaScript" },
                new Tag { Name = "SQL Server" },
                new Tag { Name = "PostgreSQL" }
            };

            context.Tags.AddRange(tags);
            await context.SaveChangesAsync();
        }
    }
}
