
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModernBlog.Models;

namespace ModernBlog.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create roles
        string[] roles = { "Admin", "Author", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // Create admin user
        var adminEmail = "admin@modernblog.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed categories if none exist
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Tecnologia", Description = "Posts sobre tecnologia e programação", Color = "#007bff" },
                new Category { Name = "Lifestyle", Description = "Posts sobre estilo de vida", Color = "#28a745" },
                new Category { Name = "Negócios", Description = "Posts sobre negócios e empreendedorismo", Color = "#ffc107" },
                new Category { Name = "Educação", Description = "Posts educacionais", Color = "#17a2b8" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed tags if none exist
        if (!await context.Tags.AnyAsync())
        {
            var tags = new[]
            {
                new Tag { Name = "C#" },
                new Tag { Name = "ASP.NET Core" },
                new Tag { Name = "JavaScript" },
                new Tag { Name = "React" },
                new Tag { Name = "Programming" },
                new Tag { Name = "Web Development" },
                new Tag { Name = "Tutorial" },
                new Tag { Name = "Tips" }
            };

            context.Tags.AddRange(tags);
            await context.SaveChangesAsync();
        }

        // Seed sample posts if none exist
        if (!await context.Posts.AnyAsync())
        {
            var category = await context.Categories.FirstAsync();
            var samplePosts = new[]
            {
                new Post
                {
                    Title = "Bem-vindo ao ModernBlog",
                    Content = "<p>Este é o primeiro post do nosso blog moderno. Aqui você encontrará conteúdo de qualidade sobre tecnologia, programação e muito mais.</p>",
                    Summary = "Post de boas-vindas ao nosso blog",
                    IsPublished = true,
                    IsFeatured = true,
                    PublishedAt = DateTime.UtcNow,
                    AuthorId = adminUser.Id,
                    CategoryId = category.Id,
                    ViewCount = 10,
                    LikeCount = 5
                }
            };

            context.Posts.AddRange(samplePosts);
            await context.SaveChangesAsync();
        }
    }
}
