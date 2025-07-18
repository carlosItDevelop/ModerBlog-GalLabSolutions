
using Microsoft.EntityFrameworkCore;
using ModernBlog.Data;
using ModernBlog.Models;
using Npgsql;

namespace ModernBlog.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (PostgresException ex) when (ex.SqlState == "57P01" && i < maxRetries - 1)
            {
                Console.WriteLine($"🔄 Conexão perdida (tentativa {i + 1}/{maxRetries}), reconectando...");
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
                
                // Force a new connection
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
            }
            catch (Exception ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "57P01" && i < maxRetries - 1)
            {
                Console.WriteLine($"🔄 Conexão perdida (tentativa {i + 1}/{maxRetries}), reconectando...");
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
            }
        }
        
        // Last attempt without retry
        return await operation();
    }

    public async Task<IEnumerable<Post>> GetPublishedPostsAsync(int page = 1, int pageSize = 6)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetFeaturedPostsAsync(int count = 5)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished && p.IsFeatured)
            .OrderByDescending(p => p.PublishedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count = 5)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId, int count = 5)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished && p.CategoryId == categoryId)
            .OrderByDescending(p => p.PublishedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetRelatedPostsAsync(Guid postId, int count = 3)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null) return new List<Post>();

        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished && p.Id != postId && p.CategoryId == post.CategoryId)
            .OrderByDescending(p => p.PublishedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        try
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao buscar post: {ex.Message}");
            // Try simple query without includes as fallback
            return await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
        }
    }

    public async Task<Post?> GetPostBySlugAsync(string slug)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.Comments.Where(c => c.IsApproved))
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.IsPublished && p.Title.ToLower().Replace(" ", "-") == slug);
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tag>> GetTagsAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            Console.WriteLine("💾 Iniciando criação do post...");
            
            post.UpdatedAt = DateTime.UtcNow;
            if (post.IsPublished && post.PublishedAt == null)
            {
                post.PublishedAt = DateTime.UtcNow;
            }

            Console.WriteLine("📝 Adicionando post ao contexto...");
            _context.Posts.Add(post);
            
            Console.WriteLine("💽 Salvando no banco...");
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Post criado com sucesso! ID: {post.Id}");
            return post;
        });
    }

    public async Task<Post> UpdatePostAsync(Post post)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            Console.WriteLine($"🔄 Atualizando post ID: {post.Id}");
            
            post.UpdatedAt = DateTime.UtcNow;
            if (post.IsPublished && post.PublishedAt == null)
            {
                post.PublishedAt = DateTime.UtcNow;
            }

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            
            Console.WriteLine("✅ Post atualizado com sucesso!");
            return post;
        });
    }

    public async Task DeletePostAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementViewCountAsync(Guid postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post != null)
        {
            post.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ToggleLikeAsync(Guid postId, Guid userId)
    {
        var existingLike = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);

        if (existingLike != null)
        {
            _context.PostLikes.Remove(existingLike);
            var post = await _context.Posts.FindAsync(postId);
            if (post != null) post.LikeCount--;
        }
        else
        {
            _context.PostLikes.Add(new PostLike { PostId = postId, UserId = userId });
            var post = await _context.Posts.FindAsync(postId);
            if (post != null) post.LikeCount++;
        }

        await _context.SaveChangesAsync();
        return existingLike == null;
    }

    public async Task<int> GetTotalPostsCountAsync()
    {
        return await _context.Posts.CountAsync(p => p.IsPublished);
    }

    public async Task<int> GetTotalPostsAsync()
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Posts.CountAsync();
        });
    }

    public async Task<int> GetPublishedPostsCountAsync()
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Posts.CountAsync(p => p.IsPublished);
        });
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
