using Student.Domain.Entities;

namespace Student.Domain.Repositories;

/// <summary>
/// Repository interface cho Student entity
/// </summary>
public interface IStudentRepository : IRepository<StudentEntity>
{
    Task<StudentEntity?> GetByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentEntity>> GetByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentEntity>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentEntity>> GetActiveStudentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentEntity>> SearchStudentsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> IsStudentCodeExistsAsync(string studentCode, CancellationToken cancellationToken = default);
    Task<int> GetTotalActiveStudentsAsync(CancellationToken cancellationToken = default);
}
