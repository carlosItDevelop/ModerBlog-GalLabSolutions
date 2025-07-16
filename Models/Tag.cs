
using System.ComponentModel.DataAnnotations;

namespace ModernBlog.Models;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

    public string Slug => Name.ToLower().Replace(" ", "-");
}
