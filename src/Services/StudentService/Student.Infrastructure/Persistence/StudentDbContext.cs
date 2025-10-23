using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;

namespace Student.Infrastructure.Persistence;

/// <summary>
/// DbContext cho Student Service
/// </summary>
public class StudentDbContext : DbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options)
    {
    }

    public DbSet<StudentEntity> Students { get; set; } = null!;
    public DbSet<Parent> Parents { get; set; } = null!;
    public DbSet<ParentStudent> ParentStudents { get; set; } = null!;
    public DbSet<Class> Classes { get; set; } = null!;
    public DbSet<ClassStudent> ClassStudents { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudentDbContext).Assembly);

        // Global query filters for soft delete
        modelBuilder.Entity<StudentEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Parent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Class>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Grade>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Có thể thêm logic trước khi save (audit, etc.)
        return base.SaveChangesAsync(cancellationToken);
    }
}
