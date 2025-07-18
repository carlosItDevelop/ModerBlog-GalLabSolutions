
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernBlog.Models;

namespace ModernBlog.Data.Mappings;

public class PostLikeMap : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        // Tabela
        builder.ToTable("PostLike");

        // Chave Primária Composta
        builder.HasKey(x => new { x.PostId, x.UserId });

        // Relacionamentos
        builder.HasOne(x => x.Post)
            .WithMany(x => x.PostLikes)
            .HasForeignKey(x => x.PostId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.PostLikes)
            .HasForeignKey(x => x.UserId);
    }
}
