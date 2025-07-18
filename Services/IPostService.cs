
using ModernBlog.Models;

namespace ModernBlog.Services;

public interface IPostService
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync(int page = 1, int pageSize = 6);
    Task<IEnumerable<Post>> GetFeaturedPostsAsync(int count = 5);
    Task<IEnumerable<Post>> GetRecentPostsAsync(int count = 5);
    Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId, int count = 5);
    Task<IEnumerable<Post>> GetRelatedPostsAsync(Guid postId, int count = 3);
    Task<Post?> GetPostByIdAsync(Guid id);
    Task<Post?> GetPostBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<IEnumerable<Tag>> GetTagsAsync();
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(Post post);
    Task DeletePostAsync(Guid id);
    Task IncrementViewCountAsync(Guid postId);
    Task<bool> ToggleLikeAsync(Guid postId, Guid userId);
    Task<int> GetTotalPostsCountAsync();
    Task<int> GetTotalPostsAsync();
    Task<int> GetPublishedPostsCountAsync();
    Task<IEnumerable<Post>> GetAllPostsAsync(int page = 1, int pageSize = 10);
}
