
using System.ComponentModel.DataAnnotations;

namespace ModernBlog.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentCommentId { get; set; }

    // Navigation Properties
    public virtual Post Post { get; set; } = null!;
    public virtual ApplicationUser Author { get; set; } = null!;
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
