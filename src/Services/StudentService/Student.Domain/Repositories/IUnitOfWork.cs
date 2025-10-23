namespace Student.Domain.Repositories;

/// <summary>
/// Unit of Work pattern để quản lý transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    public IStudentRepository Students { get; }
    public IParentRepository Parents { get; }
    public IClassRepository Classes { get; }
    public IGradeRepository Grades { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
