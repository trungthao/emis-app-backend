using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;

namespace Student.Infrastructure.Persistence.Configurations;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClassName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ClassCode)
            .HasMaxLength(20);

        builder.HasIndex(c => c.ClassCode)
            .IsUnique()
            .HasFilter("[ClassCode] IS NOT NULL");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.Room)
            .HasMaxLength(50);

        builder.Property(c => c.HeadTeacherName)
            .HasMaxLength(100);

        builder.HasOne(c => c.Grade)
            .WithMany(g => g.Classes)
            .HasForeignKey(c => c.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
