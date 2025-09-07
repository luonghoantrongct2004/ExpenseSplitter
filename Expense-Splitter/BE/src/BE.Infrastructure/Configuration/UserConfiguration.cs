using BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BE.Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            // Relationships
            builder.HasMany(u => u.ExpensesPaid)
                .WithOne(e => e.PaidBy)
                .HasForeignKey(e => e.PaidById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.SettlementsFrom)
                .WithOne(s => s.FromUser)
                .HasForeignKey(s => s.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.SettlementsTo)
                .WithOne(s => s.ToUser)
                .HasForeignKey(s => s.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}