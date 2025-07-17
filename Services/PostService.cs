
using Microsoft.EntityFrameworkCore;
using ModernBlog.Data;
using ModernBlog.Models;

namespace ModernBlog.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
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
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.Comments.Where(c => c.IsApproved))
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
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
        post.UpdatedAt = DateTime.UtcNow;
        if (post.IsPublished && post.PublishedAt == null)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<Post> UpdatePostAsync(Post post)
    {
        post.UpdatedAt = DateTime.UtcNow;
        if (post.IsPublished && post.PublishedAt == null)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return post;
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
        return await _context.Posts.CountAsync();
    }

    public async Task<int> GetPublishedPostsCountAsync()
    {
        return await _context.Posts.CountAsync(p => p.IsPublished);
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
