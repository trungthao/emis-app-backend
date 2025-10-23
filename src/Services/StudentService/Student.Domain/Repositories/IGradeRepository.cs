using Student.Domain.Entities;

namespace Student.Domain.Repositories;

/// <summary>
/// Repository interface cho Grade entity
/// </summary>
public interface IGradeRepository : IRepository<Grade>
{
    Task<Grade?> GetByGradeCodeAsync(string gradeCode, CancellationToken cancellationToken = default);
    Task<Grade?> GetByLevelAsync(int level, CancellationToken cancellationToken = default);
    Task<IEnumerable<Grade>> GetActiveGradesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Grade>> GetGradesOrderedByLevelAsync(CancellationToken cancellationToken = default);
}
