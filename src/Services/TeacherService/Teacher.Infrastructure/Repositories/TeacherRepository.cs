using Teacher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Teacher.Domain.Repositories;

namespace Teacher.Infrastructure.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TeacherDbContext _db;

        public TeacherRepository(TeacherDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Teacher.Domain.Entities.Teacher teacher, CancellationToken cancellationToken = default)
        {
            await _db.Teachers.AddAsync(teacher, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var t = await _db.Teachers.FindAsync(new object[] { id }, cancellationToken);
            if (t != null)
            {
                _db.Teachers.Remove(t);
            }
        }

        public async Task<IEnumerable<Teacher.Domain.Entities.Teacher>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Teachers.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Teacher.Domain.Entities.Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Teachers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task UpdateAsync(Teacher.Domain.Entities.Teacher teacher, CancellationToken cancellationToken = default)
        {
            _db.Teachers.Update(teacher);
            await Task.CompletedTask;
        }

        public async Task<bool> IsPhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            return await _db.Teachers.AnyAsync(t => t.Phone == phone, cancellationToken);
        }

        // Assignment related implementations
        public async Task AddAssignmentAsync(Teacher.Domain.Entities.TeacherClassAssignment assignment, CancellationToken cancellationToken = default)
        {
            await _db.TeacherClassAssignments.AddAsync(assignment, cancellationToken);
        }

        public async Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default)
        {
            var a = await _db.TeacherClassAssignments.FindAsync(new object[] { assignmentId }, cancellationToken);
            if (a != null)
            {
                _db.TeacherClassAssignments.Remove(a);
            }
        }

        public async Task<IEnumerable<Teacher.Domain.Entities.TeacherClassAssignment>> GetAssignmentsByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
        {
            return await _db.TeacherClassAssignments.AsNoTracking()
                .Where(a => a.TeacherId == teacherId)
                .ToListAsync(cancellationToken);
        }

        // ClassInfo local replica implementation (for production scalability)
        public async Task<Teacher.Domain.Entities.ClassInfo?> GetClassInfoByIdAsync(Guid classId, CancellationToken cancellationToken = default)
        {
            return await _db.ClassInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClassId == classId, cancellationToken);
        }

        public async Task<IEnumerable<Teacher.Domain.Entities.ClassInfo>> GetClassInfosByIdsAsync(IEnumerable<Guid> classIds, CancellationToken cancellationToken = default)
        {
            var classIdList = classIds.ToList();
            return await _db.ClassInfos
                .AsNoTracking()
                .Where(c => classIdList.Contains(c.ClassId))
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertClassInfoAsync(Teacher.Domain.Entities.ClassInfo classInfo, CancellationToken cancellationToken = default)
        {
            var existing = await _db.ClassInfos
                .FirstOrDefaultAsync(c => c.ClassId == classInfo.ClassId, cancellationToken);

            if (existing != null)
            {
                // Update existing
                existing.UpdateInfo(
                    classInfo.ClassName,
                    classInfo.Grade,
                    classInfo.AcademicYear,
                    classInfo.TotalStudents,
                    classInfo.SchoolId
                );
            }
            else
            {
                // Insert new
                await _db.ClassInfos.AddAsync(classInfo, cancellationToken);
            }
        }
    }
}
