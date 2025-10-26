using Microsoft.EntityFrameworkCore;
// using Teacher.Domain.Entities; // use fully-qualified names to avoid namespace/type conflicts

namespace Teacher.Infrastructure.Persistence
{
    public class TeacherDbContext : DbContext
    {
        public TeacherDbContext(DbContextOptions<TeacherDbContext> options) : base(options) { }

        public DbSet<Teacher.Domain.Entities.Teacher> Teachers { get; set; } = null!;
        public DbSet<Teacher.Domain.Entities.TeacherClassAssignment> TeacherClassAssignments { get; set; } = null!;
        public DbSet<Teacher.Domain.Entities.ClassInfo> ClassInfos { get; set; } = null!; // Local replica

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Teacher.Domain.Entities.Teacher>(entity =>
            {
                entity.ToTable("Teachers");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(t => t.LastName).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Phone).HasMaxLength(20);
                entity.Property(t => t.Email).HasMaxLength(200);
                entity.Property(t => t.Address).HasMaxLength(500);
            });

            modelBuilder.Entity<Teacher.Domain.Entities.TeacherClassAssignment>(entity =>
            {
                entity.ToTable("TeacherClassAssignments");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.TeacherId).IsRequired();
                entity.Property(a => a.ClassId).IsRequired();
                entity.Property(a => a.AssignedAt).IsRequired();
                entity.Property(a => a.Role).HasMaxLength(100);
            });

            modelBuilder.Entity<Teacher.Domain.Entities.ClassInfo>(entity =>
            {
                entity.ToTable("ClassInfos");
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.ClassId).IsUnique(); // ClassId from external service is unique
                entity.Property(c => c.ClassId).IsRequired();
                entity.Property(c => c.ClassName).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Grade).HasMaxLength(50);
                entity.Property(c => c.AcademicYear).HasMaxLength(20);
                entity.Property(c => c.SyncSource).HasMaxLength(100);
                entity.Property(c => c.LastSyncedAt).IsRequired();
            });
        }
    }
}
