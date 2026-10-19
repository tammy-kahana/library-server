using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        // Restrict: היסטוריית ההשאלות לא נמחקת כשמוחקים קוראת או עותק
        builder.HasOne(l => l.User)
               .WithMany(u => u.Loans)
               .HasForeignKey(l => l.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.BookCopy)
               .WithMany(c => c.Loans)
               .HasForeignKey(l => l.BookCopyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
