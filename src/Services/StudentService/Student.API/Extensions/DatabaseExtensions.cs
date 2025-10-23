using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Infrastructure.Persistence;

namespace Student.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting database migration...");
            
            // Apply pending migrations
            await context.Database.MigrateAsync();
            
            logger.LogInformation("Database migration completed successfully");

            // Seed data if database is empty
            if (!await context.Grades.AnyAsync())
            {
                logger.LogInformation("Seeding initial data...");
                await SeedDataAsync(context);
                logger.LogInformation("Data seeding completed successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    private static async Task SeedDataAsync(StudentDbContext context)
    {
        // Seed Grades - Khối lớp theo độ tuổi
        var grades = new List<Grade>
        {
            new Grade("Nhà trẻ", "NT", 1, 18, 36),      // 18-36 tháng tuổi
            new Grade("Mẫu giáo nhỏ", "MGN", 2, 3, 4),  // 3-4 tuổi
            new Grade("Mẫu giáo lớn", "MGL", 3, 4, 5),  // 4-5 tuổi
            new Grade("Lá", "LA", 4, 5, 6),             // 5-6 tuổi
        };
        await context.Grades.AddRangeAsync(grades);
        await context.SaveChangesAsync();

        // Seed Classes - Không cần năm học
        var classes = new List<Class>
        {
            new Class("Lớp Nhà trẻ A", "NTA", grades[0].Id, 20),
            new Class("Lớp Nhà trẻ B", "NTB", grades[0].Id, 20),
            new Class("Lớp Mẫu giáo nhỏ A", "MGNA", grades[1].Id, 25),
            new Class("Lớp Mẫu giáo nhỏ B", "MGNB", grades[1].Id, 25),
            new Class("Lớp Mẫu giáo lớn A", "MGLA", grades[2].Id, 25),
            new Class("Lớp Mẫu giáo lớn B", "MGLB", grades[2].Id, 25),
            new Class("Lớp Lá A", "LAA", grades[3].Id, 30),
            new Class("Lớp Lá B", "LAB", grades[3].Id, 30)
        };
        await context.Classes.AddRangeAsync(classes);
        await context.SaveChangesAsync();

        // Seed Sample Students
        var sampleStudents = new List<StudentEntity>
        {
            new StudentEntity("HS001", "Văn A", "Nguyễn", new DateTime(2018, 5, 15), Gender.Male, DateTime.Now),
            new StudentEntity("HS002", "Thị B", "Trần", new DateTime(2018, 8, 20), Gender.Female, DateTime.Now),
            new StudentEntity("HS003", "Văn C", "Lê", new DateTime(2018, 3, 10), Gender.Male, DateTime.Now),
            new StudentEntity("HS004", "Thị D", "Phạm", new DateTime(2018, 12, 5), Gender.Female, DateTime.Now),
            new StudentEntity("HS005", "Văn E", "Hoàng", new DateTime(2018, 7, 25), Gender.Male, DateTime.Now)
        };

        // Assign students to classes
        sampleStudents[0].AssignToClass(classes[0].Id);
        sampleStudents[1].AssignToClass(classes[0].Id);
        sampleStudents[2].AssignToClass(classes[1].Id);
        sampleStudents[3].AssignToClass(classes[2].Id);
        sampleStudents[4].AssignToClass(classes[2].Id);

        await context.Students.AddRangeAsync(sampleStudents);
        await context.SaveChangesAsync();

        // Seed Sample Parents
        var sampleParents = new List<Parent>
        {
            new Parent("Văn X", "Nguyễn", Gender.Male, "0901234567"),
            new Parent("Thị Y", "Nguyễn", Gender.Female, "0901234568"),
            new Parent("Văn M", "Trần", Gender.Male, "0901234569"),
            new Parent("Thị N", "Trần", Gender.Female, "0901234570")
        };

        await context.Parents.AddRangeAsync(sampleParents);
        await context.SaveChangesAsync();

        // Link Parents to Students
        var parentStudentRelations = new List<ParentStudent>
        {
            new ParentStudent(sampleParents[0].Id, sampleStudents[0].Id, RelationshipType.Father, true),
            new ParentStudent(sampleParents[1].Id, sampleStudents[0].Id, RelationshipType.Mother, false),
            new ParentStudent(sampleParents[2].Id, sampleStudents[1].Id, RelationshipType.Father, true),
            new ParentStudent(sampleParents[3].Id, sampleStudents[1].Id, RelationshipType.Mother, false)
        };

        await context.ParentStudents.AddRangeAsync(parentStudentRelations);
        await context.SaveChangesAsync();
    }
}
