
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

    public async Task<IEnumerable<Post>> GetPublishedPostsAsync(int page = 1, int pageSize = 10)
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

    public async Task<IEnumerable<Post>> GetMostViewedPostsAsync(int count = 5)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.IsPublished && p.CategoryId == categoryId)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .FirstOrDefaultAsync(p => p.IsPublished && p.Title.ToLower().Replace(" ", "-") == slug.ToLower());
    }

    public async Task<IEnumerable<Post>> GetRelatedPostsAsync(Guid postId, int count = 3)
    {
        var post = await _context.Posts
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null) return new List<Post>();

        var tagIds = post.PostTags.Select(pt => pt.TagId).ToList();

        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished && p.Id != postId && 
                   p.PostTags.Any(pt => tagIds.Contains(pt.TagId)))
            .OrderByDescending(p => p.PublishedAt)
            .Take(count)
            .ToListAsync();
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

    public async Task<bool> ToggleLikeAsync(Guid postId, string? ipAddress, Guid? userId = null)
    {
        var existingLike = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostId == postId && 
                                     ((userId.HasValue && pl.UserId == userId) || 
                                      (!userId.HasValue && pl.IpAddress == ipAddress)));

        if (existingLike != null)
        {
            _context.PostLikes.Remove(existingLike);
            var post = await _context.Posts.FindAsync(postId);
            if (post != null) post.LikeCount--;
            await _context.SaveChangesAsync();
            return false;
        }
        else
        {
            var newLike = new PostLike
            {
                PostId = postId,
                UserId = userId,
                IpAddress = ipAddress
            };
            _context.PostLikes.Add(newLike);
            var post = await _context.Posts.FindAsync(postId);
            if (post != null) post.LikeCount++;
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public async Task<int> GetTotalPostsCountAsync()
    {
        return await _context.Posts.CountAsync();
    }

    public async Task<int> GetTotalPublishedPostsCountAsync()
    {
        return await _context.Posts.CountAsync(p => p.IsPublished);
    }
}
