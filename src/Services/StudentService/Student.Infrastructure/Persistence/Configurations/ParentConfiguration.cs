using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;

namespace Student.Infrastructure.Persistence.Configurations;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Parents");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Gender)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(15);

        builder.HasIndex(p => p.Phone);

        builder.Property(p => p.Email)
            .HasMaxLength(100);

        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.Property(p => p.Address)
            .HasMaxLength(500);

        builder.Property(p => p.Occupation)
            .HasMaxLength(100);

        builder.Property(p => p.WorkPlace)
            .HasMaxLength(200);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(p => p.UserId)
            .HasMaxLength(50);

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        builder.HasMany(p => p.Students)
            .WithOne(ps => ps.Parent)
            .HasForeignKey(ps => ps.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
