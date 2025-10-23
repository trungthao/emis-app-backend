using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;

namespace Student.Infrastructure.Persistence.Configurations;

public class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
{
    public void Configure(EntityTypeBuilder<ParentStudent> builder)
    {
        builder.ToTable("ParentStudents");

        builder.HasKey(ps => ps.Id);

        builder.HasIndex(ps => new { ps.ParentId, ps.StudentId })
            .IsUnique();

        builder.Property(ps => ps.RelationshipType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ps => ps.IsPrimaryContact)
            .IsRequired();

        builder.Property(ps => ps.CanPickUp)
            .IsRequired();

        builder.Property(ps => ps.ReceiveNotifications)
            .IsRequired();

        builder.HasOne(ps => ps.Parent)
            .WithMany(p => p.Students)
            .HasForeignKey(ps => ps.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Student)
            .WithMany(s => s.Parents)
            .HasForeignKey(ps => ps.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
