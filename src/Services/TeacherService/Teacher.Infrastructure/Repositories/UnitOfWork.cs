using Teacher.Domain.Repositories;
using Teacher.Infrastructure.Persistence;

namespace Teacher.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TeacherDbContext _context;

        public UnitOfWork(TeacherDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
