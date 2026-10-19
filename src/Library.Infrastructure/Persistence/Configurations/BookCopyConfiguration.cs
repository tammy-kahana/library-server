using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.Property(c => c.Barcode).IsRequired().HasMaxLength(50);

        builder.HasIndex(c => c.Barcode).IsUnique();

        // Cascade: מחיקת ספר מוחקת את העותקים שלו.
        // עותק עם היסטוריית השאלות מוגן ב-Restrict שב-LoanConfiguration.
        builder.HasOne(c => c.Book)
               .WithMany(b => b.Copies)
               .HasForeignKey(c => c.BookId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
