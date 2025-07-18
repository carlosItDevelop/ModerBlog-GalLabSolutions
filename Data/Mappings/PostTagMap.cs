
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernBlog.Models;

namespace ModernBlog.Data.Mappings;

public class PostTagMap : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        // Tabela
        builder.ToTable("PostTag");

        // Chave Primária Composta
        builder.HasKey(x => new { x.PostId, x.TagId });

        // Relacionamentos
        builder.HasOne(x => x.Post)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.PostId);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.TagId);
    }
}
