using BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BE.Infrastructure.Configuration
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("expenses");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("VND");

            builder.Property(e => e.Category)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(e => e.GroupId);
            builder.HasIndex(e => e.ExpenseDate);
            builder.HasIndex(e => new { e.GroupId, e.ExpenseDate });

            // Relationships
            builder.HasOne(e => e.Group)
                .WithMany(g => g.Expenses)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.PaidBy)
                .WithMany(u => u.ExpensesPaid)
                .HasForeignKey(e => e.PaidById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}