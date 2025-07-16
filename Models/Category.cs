
using System.ComponentModel.DataAnnotations;

namespace ModernBlog.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public string? Color { get; set; } = "#007bff";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public string Slug => Name.ToLower().Replace(" ", "-");
}
