
namespace ModernBlog.Models;

public class PostLike
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PostId { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Post Post { get; set; } = null!;
    public virtual ApplicationUser? User { get; set; }
}
