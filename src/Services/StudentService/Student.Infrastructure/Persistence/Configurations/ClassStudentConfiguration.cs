using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;

namespace Student.Infrastructure.Persistence.Configurations;

public class ClassStudentConfiguration : IEntityTypeConfiguration<ClassStudent>
{
    public void Configure(EntityTypeBuilder<ClassStudent> builder)
    {
        builder.ToTable("ClassStudents");

        builder.HasKey(cs => cs.Id);

        builder.HasIndex(cs => new { cs.ClassId, cs.StudentId });

        builder.Property(cs => cs.JoinDate)
            .IsRequired();

        builder.Property(cs => cs.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(cs => cs.Notes)
            .HasMaxLength(1000);

        builder.HasOne(cs => cs.Class)
            .WithMany(c => c.ClassStudents)
            .HasForeignKey(cs => cs.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Student)
            .WithMany(s => s.ClassHistory)
            .HasForeignKey(cs => cs.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
