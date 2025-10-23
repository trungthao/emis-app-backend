using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Domain.Repositories;
using Student.Infrastructure.Persistence;

namespace Student.Infrastructure.Repositories;

public class ClassRepository : Repository<Class>, IClassRepository
{
    public ClassRepository(StudentDbContext context) : base(context)
    {
    }

    public async Task<Class?> GetByClassCodeAsync(string classCode, CancellationToken cancellationToken = default)
    {
        return await _context.Classes
            .Include(c => c.Grade)
            .FirstOrDefaultAsync(c => c.ClassCode == classCode, cancellationToken);
    }

    public async Task<IEnumerable<Class>> GetByGradeIdAsync(Guid gradeId, CancellationToken cancellationToken = default)
    {
        return await _context.Classes
            .Include(c => c.Grade)
            .Where(c => c.GradeId == gradeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Class>> GetByHeadTeacherIdAsync(Guid headTeacherId, CancellationToken cancellationToken = default)
    {
        return await _context.Classes
            .Include(c => c.Grade)
            .Where(c => c.HeadTeacherId == headTeacherId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Class>> GetActiveClassesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Classes
            .Include(c => c.Grade)
            .Where(c => c.Status == ClassStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Class>> GetAvailableClassesForEnrollmentAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Classes
            .Include(c => c.Grade)
            .Where(c => c.Status == ClassStatus.Active && 
                       c.CurrentStudentCount < c.Capacity)
            .ToListAsync(cancellationToken);
    }
}
