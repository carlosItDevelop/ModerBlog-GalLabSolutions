
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

        // Add more sample posts
        var allCategories = await context.Categories.ToListAsync();
        var techCategory = allCategories.First(c => c.Name == "Tecnologia");
        var lifestyleCategory = allCategories.First(c => c.Name == "Lifestyle");
        var businessCategory = allCategories.First(c => c.Name == "Negócios");
        var educationCategory = allCategories.First(c => c.Name == "Educação");

        // Check if we need to add more posts (add if less than 10 posts exist)
        var currentPostCount = await context.Posts.CountAsync();
        if (currentPostCount < 10)
        {
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
                    PublishedAt = DateTime.UtcNow.AddDays(-10),
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
                    PublishedAt = DateTime.UtcNow.AddDays(-8),
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
                    PublishedAt = DateTime.UtcNow.AddDays(-6),
                    AuthorId = adminUser.Id,
                    CategoryId = lifestyleCategory.Id,
                    ViewCount = 32,
                    LikeCount = 8
                },
                new Post
                {
                    Title = "Como Começar sua Carreira em Programação",
                    Content = "<p>Entrar no mundo da programação pode parecer desafiador no início, mas com as estratégias certas, qualquer pessoa pode ter sucesso.</p><p>Neste guia completo, exploramos os passos fundamentais para iniciar sua jornada como desenvolvedor.</p><p>Desde a escolha da primeira linguagem até dicas para conseguir o primeiro emprego na área.</p>",
                    Summary = "Guia completo para iniciantes em programação",
                    IsPublished = true,
                    IsFeatured = false,
                    FeaturedImageUrl = "/images/imagem1.png",
                    PublishedAt = DateTime.UtcNow.AddDays(-5),
                    AuthorId = adminUser.Id,
                    CategoryId = educationCategory.Id,
                    ViewCount = 156,
                    LikeCount = 34
                },
                new Post
                {
                    Title = "O Futuro do Trabalho Remoto",
                    Content = "<p>O trabalho remoto transformou-se de uma tendência para uma realidade permanente em muitas empresas.</p><p>Analisamos como essa mudança está impactando diferentes setores e quais são as melhores práticas para prosperar neste novo ambiente.</p><p>Descubra ferramentas, técnicas e estratégias para maximizar sua produtividade trabalhando de casa.</p>",
                    Summary = "Explorando o impacto e as oportunidades do trabalho remoto",
                    IsPublished = true,
                    IsFeatured = false,
                    FeaturedImageUrl = "/images/imagem2.png",
                    PublishedAt = DateTime.UtcNow.AddDays(-4),
                    AuthorId = adminUser.Id,
                    CategoryId = businessCategory.Id,
                    ViewCount = 89,
                    LikeCount = 19
                },
                new Post
                {
                    Title = "Introdução ao ASP.NET Core",
                    Content = "<p>ASP.NET Core é uma das frameworks mais poderosas para desenvolvimento web com C#.</p><p>Neste tutorial, você aprenderá os conceitos básicos e como criar sua primeira aplicação web.</p><p>Cobriremos desde a configuração do ambiente até a criação de controllers e views.</p>",
                    Summary = "Tutorial completo para começar com ASP.NET Core",
                    IsPublished = true,
                    IsFeatured = false,
                    FeaturedImageUrl = "/images/imagem3.png",
                    PublishedAt = DateTime.UtcNow.AddDays(-3),
                    AuthorId = adminUser.Id,
                    CategoryId = techCategory.Id,
                    ViewCount = 203,
                    LikeCount = 45
                },
                new Post
                {
                    Title = "Melhores Práticas de UI/UX",
                    Content = "<p>Um bom design é fundamental para o sucesso de qualquer aplicação web.</p><p>Exploramos as principais tendências em UI/UX e como aplicá-las em seus projetos.</p><p>Desde a escolha de cores até a organização da informação na tela.</p>",
                    Summary = "Guia essencial para criar interfaces mais atrativas e funcionais",
                    IsPublished = true,
                    IsFeatured = false,
                    FeaturedImageUrl = "/images/imagem1.png",
                    PublishedAt = DateTime.UtcNow.AddDays(-2),
                    AuthorId = adminUser.Id,
                    CategoryId = educationCategory.Id,
                    ViewCount = 125,
                    LikeCount = 28
                },
                new Post
                {
                    Title = "Gerenciamento de Projetos Ágeis",
                    Content = "<p>A metodologia ágil revolucionou a forma como desenvolvemos software.</p><p>Aprenda os princípios fundamentais do Scrum e Kanban e como aplicá-los em seus projetos.</p><p>Descubra ferramentas e técnicas para melhorar a colaboração em equipe.</p>",
                    Summary = "Dominando metodologias ágeis para maior eficiência",
                    IsPublished = true,
                    IsFeatured = false,
                    FeaturedImageUrl = "/images/imagem2.png",
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    AuthorId = adminUser.Id,
                    CategoryId = businessCategory.Id,
                    ViewCount = 67,
                    LikeCount = 15
                },
                new Post
                {
                    Title = "Segurança em Aplicações Web",
                    Content = "<p>A segurança deve ser uma prioridade em qualquer aplicação web moderna.</p><p>Neste post, cobrimos as principais vulnerabilidades e como proteger suas aplicações.</p><p>Desde autenticação segura até proteção contra ataques comuns como XSS e SQL Injection.</p>",
                    Summary = "Protegendo suas aplicações contra ameaças digitais",
                    IsPublished = true,
                    IsFeatured = true,
                    FeaturedImageUrl = "/images/imagem3.png",
                    PublishedAt = DateTime.UtcNow,
                    AuthorId = adminUser.Id,
                    CategoryId = techCategory.Id,
                    ViewCount = 12,
                    LikeCount = 3
                }
            };

            // Only add posts that don't already exist (check by title)
            var existingTitles = await context.Posts.Select(p => p.Title).ToListAsync();
            var newPosts = samplePosts.Where(p => !existingTitles.Contains(p.Title)).ToArray();
            
            if (newPosts.Any())
            {
                context.Posts.AddRange(newPosts);
                await context.SaveChangesAsync();
            }
        }
    }
}
