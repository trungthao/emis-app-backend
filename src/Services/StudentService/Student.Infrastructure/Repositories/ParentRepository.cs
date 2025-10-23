using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Domain.Repositories;
using Student.Infrastructure.Persistence;

namespace Student.Infrastructure.Repositories;

public class ParentRepository : Repository<Parent>, IParentRepository
{
    public ParentRepository(StudentDbContext context) : base(context)
    {
    }

    public async Task<Parent?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await _context.Parents
            .FirstOrDefaultAsync(p => p.Phone == phone, cancellationToken);
    }

    public async Task<Parent?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Parents
            .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
    }

    public async Task<Parent?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Parents
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Parent>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Parents
            .Where(p => p.Students.Any(ps => ps.StudentId == studentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Parent>> SearchParentsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        searchTerm = searchTerm.ToLower();
        return await _context.Parents
            .Where(p => p.FirstName.ToLower().Contains(searchTerm) ||
                       p.LastName.ToLower().Contains(searchTerm) ||
                       p.Phone.Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }
}
