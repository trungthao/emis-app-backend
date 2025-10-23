using Student.Domain.Entities;

namespace Student.Domain.Repositories;

/// <summary>
/// Repository interface cho Parent entity
/// </summary>
public interface IParentRepository : IRepository<Parent>
{
    Task<Parent?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<Parent?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Parent?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Parent>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Parent>> SearchParentsAsync(string searchTerm, CancellationToken cancellationToken = default);
}
