using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            
            // Convert List<string> to JSON for MySQL
            entity.Property(e => e.Roles)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            // Indexes
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.RefreshToken);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var teacherId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var parentId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = adminId,
                Username = "admin",
                Email = "admin@emis.edu.vn",
                PasswordHash = "admin123", // TODO: Hash in production
                FullName = "Nguyễn Văn Admin",
                PhoneNumber = "0901234567",
                Roles = new List<string> { "Admin" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = teacherId,
                Username = "teacher",
                Email = "teacher@emis.edu.vn",
                PasswordHash = "teacher123", // TODO: Hash in production
                FullName = "Trần Thị Giáo Viên",
                PhoneNumber = "0902345678",
                Roles = new List<string> { "Teacher" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = parentId,
                Username = "parent",
                Email = "parent@emis.edu.vn",
                PasswordHash = "parent123", // TODO: Hash in production
                FullName = "Lê Văn Phụ Huynh",
                PhoneNumber = "0903456789",
                Roles = new List<string> { "Parent" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
