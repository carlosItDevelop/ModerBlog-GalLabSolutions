
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
            var categories = await context.Categories.ToListAsync();
            var techCategory = categories.First(c => c.Name == "Tecnologia");
            var lifestyleCategory = categories.First(c => c.Name == "Lifestyle");
            var businessCategory = categories.First(c => c.Name == "Negócios");

            var samplePosts = new[]
            {
                new Post
                {
                    Title = "Bem-vindo ao ModernBlog",
                    Content = "<p>Este é o primeiro post do nosso blog moderno. Aqui você encontrará conteúdo de qualidade sobre tecnologia, programação e muito mais.</p><p>Nosso objetivo é compartilhar conhecimento e experiências que possam ajudar nossa comunidade a crescer profissionalmente.</p>",
                    Summary = "Post de boas-vindas ao nosso blog",
                    IsPublished = true,
                    IsFeatured = true,
                    FeaturedImageUrl = "/images/imagem1.png",
                    PublishedAt = DateTime.UtcNow.AddDays(-2),
                    AuthorId = adminUser.Id,
                    CategoryId = techCategory.Id,
                    ViewCount = 45,
                    LikeCount = 12
                },
                new Post
                {
                    Title = "As Tendências de Tecnologia para 2024",
                    Content = "<p>A tecnologia continua evoluindo rapidamente, e 2024 promete ser um ano repleto de inovações.</p><p>Desde inteligência artificial até desenvolvimento web moderno, exploramos as principais tendências que moldarão o futuro.</p><p>Prepare-se para descobrir as tecnologias que todo desenvolvedor deveria conhecer!</p>",
                    Summary = "Descubra as principais tendências tecnológicas que definirão 2024",
                    IsPublished = true,
                    IsFeatured = true,
                    FeaturedImageUrl = "/images/imagem2.png",
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    AuthorId = adminUser.Id,
                    CategoryId = techCategory.Id,
                    ViewCount = 78,
                    LikeCount = 23
                },
                new Post
                {
                    Title = "Dicas de Produtividade para Desenvolvedores",
                    Content = "<p>Ser produtivo como desenvolvedor vai muito além de escrever código rapidamente.</p><p>Neste post, compartilhamos estratégias práticas para otimizar seu workflow, desde a organização do ambiente de trabalho até técnicas de gerenciamento de tempo.</p><p>Aprenda como pequenas mudanças podem gerar grandes resultados na sua produtividade diária.</p>",
                    Summary = "Estratégias práticas para aumentar sua produtividade como desenvolvedor",
                    IsPublished = true,
                    IsFeatured = true,
                    FeaturedImageUrl = "/images/imagem3.png",
                    PublishedAt = DateTime.UtcNow,
                    AuthorId = adminUser.Id,
                    CategoryId = lifestyleCategory.Id,
                    ViewCount = 32,
                    LikeCount = 8
                }
            };

            context.Posts.AddRange(samplePosts);
            await context.SaveChangesAsync();
        }
    }
}
