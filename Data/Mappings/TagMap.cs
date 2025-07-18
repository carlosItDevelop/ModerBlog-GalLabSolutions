
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernBlog.Models;

namespace ModernBlog.Data.Mappings;

public class TagMap : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        // Tabela
        builder.ToTable("Tag");

        // Chave Primária
        builder.HasKey(x => x.Id);

        // Propriedades
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        // Índices
        builder.HasIndex(x => x.Name, "IX_Tag_Name").IsUnique();
    }
}
