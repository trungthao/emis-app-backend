using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;

namespace Student.Infrastructure.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.GradeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.GradeCode)
            .HasMaxLength(20);

        builder.HasIndex(g => g.GradeCode)
            .IsUnique()
            .HasFilter("[GradeCode] IS NOT NULL");

        builder.Property(g => g.Level)
            .IsRequired();

        builder.HasIndex(g => g.Level)
            .IsUnique();

        builder.Property(g => g.AgeFrom);

        builder.Property(g => g.AgeTo);

        builder.Property(g => g.Description)
            .HasMaxLength(500);

        builder.Property(g => g.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasMany(g => g.Classes)
            .WithOne(c => c.Grade)
            .HasForeignKey(c => c.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
