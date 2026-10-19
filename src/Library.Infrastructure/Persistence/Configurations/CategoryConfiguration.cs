using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(c => c.Name).IsUnique();

        // many-to-many: ספר ↔ קטגוריה. EF יוצר לבד את טבלת הקשר BookCategories
        builder.HasMany(c => c.Books)
               .WithMany(b => b.Categories)
               .UsingEntity(j => j.ToTable("BookCategories"));
    }
}
