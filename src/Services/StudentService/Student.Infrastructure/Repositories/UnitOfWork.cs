using Microsoft.EntityFrameworkCore.Storage;
using Student.Domain.Repositories;
using Student.Infrastructure.Persistence;

namespace Student.Infrastructure.Repositories;

/// <summary>
/// Implementation của IUnitOfWork
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly StudentDbContext _context;
    private IDbContextTransaction? _transaction;

    public IStudentRepository Students { get; }
    public IParentRepository Parents { get; }
    public IClassRepository Classes { get; }
    public IGradeRepository Grades { get; }

    public UnitOfWork(
        StudentDbContext context,
        IStudentRepository studentRepository,
        IParentRepository parentRepository,
        IClassRepository classRepository,
        IGradeRepository gradeRepository)
    {
        _context = context;
        Students = studentRepository;
        Parents = parentRepository;
        Classes = classRepository;
        Grades = gradeRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
