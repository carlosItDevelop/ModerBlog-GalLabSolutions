
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernBlog.Models;

namespace ModernBlog.Data.Mappings;

public class CategoryMap : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Tabela
        builder.ToTable("Category");

        // Chave Primária
        builder.HasKey(x => x.Id);

        // Propriedades
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.Color)
            .HasMaxLength(7);

        // Índices
        builder.HasIndex(x => x.Name, "IX_Category_Name").IsUnique();
    }
}
