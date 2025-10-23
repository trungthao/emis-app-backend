using Student.Domain.Entities;

namespace Student.Domain.Repositories;

/// <summary>
/// Repository interface cho Class entity
/// </summary>
public interface IClassRepository : IRepository<Class>
{
    Task<Class?> GetByClassCodeAsync(string classCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Class>> GetByGradeIdAsync(Guid gradeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Class>> GetByHeadTeacherIdAsync(Guid headTeacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Class>> GetActiveClassesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Class>> GetAvailableClassesForEnrollmentAsync(CancellationToken cancellationToken = default);
}
