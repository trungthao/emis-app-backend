using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;

namespace Student.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration cho StudentEntity
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<StudentEntity>
{
    public void Configure(EntityTypeBuilder<StudentEntity> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StudentCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(s => s.StudentCode)
            .IsUnique();

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Gender)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Phone)
            .HasMaxLength(15);

        builder.Property(s => s.Email)
            .HasMaxLength(100);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.PlaceOfBirth)
            .HasMaxLength(200);

        builder.Property(s => s.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(s => s.BloodType)
            .HasMaxLength(10);

        builder.Property(s => s.Allergies)
            .HasMaxLength(500);

        builder.Property(s => s.MedicalNotes)
            .HasMaxLength(1000);

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(s => s.CurrentClass)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.CurrentClassId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Parents)
            .WithOne(ps => ps.Student)
            .HasForeignKey(ps => ps.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.ClassHistory)
            .WithOne(cs => cs.Student)
            .HasForeignKey(cs => cs.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
