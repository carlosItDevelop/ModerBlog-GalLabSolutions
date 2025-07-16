
using ModernBlog.Models;

namespace ModernBlog.Services;

public interface IPostService
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<Post>> GetFeaturedPostsAsync(int count = 5);
    Task<IEnumerable<Post>> GetRecentPostsAsync(int count = 5);
    Task<IEnumerable<Post>> GetMostViewedPostsAsync(int count = 5);
    Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10);
    Task<Post?> GetPostByIdAsync(Guid id);
    Task<Post?> GetPostBySlugAsync(string slug);
    Task<IEnumerable<Post>> GetRelatedPostsAsync(Guid postId, int count = 3);
    Task IncrementViewCountAsync(Guid postId);
    Task<bool> ToggleLikeAsync(Guid postId, string? ipAddress, Guid? userId = null);
    Task<int> GetTotalPostsCountAsync();
    Task<int> GetTotalPublishedPostsCountAsync();
}
