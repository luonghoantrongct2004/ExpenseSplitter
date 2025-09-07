using BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BE.Infrastructure.Configuration
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.ToTable("groups");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("VND");

            builder.Property(g => g.InviteCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(g => g.InviteCode)
                .IsUnique();

            // Generate invite code automatically
            builder.Property(g => g.InviteCode)
                .HasDefaultValueSql("gen_random_uuid()::text");

            // Relationships
            builder.HasOne(g => g.CreatedBy)
                .WithMany()
                .HasForeignKey(g => g.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}