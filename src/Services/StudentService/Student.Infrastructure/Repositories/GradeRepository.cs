using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Domain.Repositories;
using Student.Infrastructure.Persistence;

namespace Student.Infrastructure.Repositories;

public class GradeRepository : Repository<Grade>, IGradeRepository
{
    public GradeRepository(StudentDbContext context) : base(context)
    {
    }

    public async Task<Grade?> GetByGradeCodeAsync(string gradeCode, CancellationToken cancellationToken = default)
    {
        return await _context.Grades
            .FirstOrDefaultAsync(g => g.GradeCode == gradeCode, cancellationToken);
    }

    public async Task<Grade?> GetByLevelAsync(int level, CancellationToken cancellationToken = default)
    {
        return await _context.Grades
            .FirstOrDefaultAsync(g => g.Level == level, cancellationToken);
    }

    public async Task<IEnumerable<Grade>> GetActiveGradesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Grades
            .Where(g => g.Status == GradeStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Grade>> GetGradesOrderedByLevelAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Grades
            .OrderBy(g => g.Level)
            .ToListAsync(cancellationToken);
    }
}
