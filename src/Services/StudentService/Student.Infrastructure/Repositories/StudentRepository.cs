using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Domain.Repositories;
using Student.Infrastructure.Persistence;

namespace Student.Infrastructure.Repositories;

/// <summary>
/// Implementation của IStudentRepository
/// </summary>
public class StudentRepository : Repository<StudentEntity>, IStudentRepository
{
    public StudentRepository(StudentDbContext context) : base(context)
    {
    }

    public async Task<StudentEntity?> GetByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.CurrentClass)
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode, cancellationToken);
    }

    public async Task<IEnumerable<StudentEntity>> GetByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.CurrentClass)
            .Where(s => s.CurrentClassId == classId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentEntity>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.CurrentClass)
            .Where(s => s.Parents.Any(ps => ps.ParentId == parentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentEntity>> GetActiveStudentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.CurrentClass)
            .Where(s => s.Status == StudentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentEntity>> SearchStudentsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        searchTerm = searchTerm.ToLower();
        return await _context.Students
            .Include(s => s.CurrentClass)
            .Where(s => s.StudentCode.ToLower().Contains(searchTerm) ||
                       s.FirstName.ToLower().Contains(searchTerm) ||
                       s.LastName.ToLower().Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsStudentCodeExistsAsync(string studentCode, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AnyAsync(s => s.StudentCode == studentCode, cancellationToken);
    }

    public async Task<int> GetTotalActiveStudentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .CountAsync(s => s.Status == StudentStatus.Active, cancellationToken);
    }
}
