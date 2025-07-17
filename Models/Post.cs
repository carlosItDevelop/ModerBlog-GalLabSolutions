
using System.ComponentModel.DataAnnotations;

namespace ModernBlog.Models;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Summary { get; set; }

    public string? FeaturedImageUrl { get; set; }

    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;

    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    // Foreign Keys
    public Guid AuthorId { get; set; }
    public Guid CategoryId { get; set; }

    // Navigation Properties
    public virtual ApplicationUser Author { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public string Slug => Title.ToLower().Replace(" ", "-").Replace("--", "-");
}
