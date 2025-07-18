
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernBlog.Models;

namespace ModernBlog.Data.Mappings;

public class PostMap : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        // Tabela
        builder.ToTable("Post");

        // Chave Primária
        builder.HasKey(x => x.Id);

        // Propriedades
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(x => x.Summary)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.IsPublished);

        builder.Property(x => x.IsFeatured);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.PublishedAt);

        builder.Property(x => x.ViewCount);

        builder.Property(x => x.LikeCount);

        // Relacionamentos
        builder.HasOne(x => x.Author)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.CategoryId);

        // Índices
        builder.HasIndex(x => x.Title);
        builder.HasIndex(x => x.IsPublished);
        builder.HasIndex(x => x.CreatedAt);
    }
}
