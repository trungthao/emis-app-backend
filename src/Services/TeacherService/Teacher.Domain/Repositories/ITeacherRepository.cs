namespace Teacher.Domain.Repositories
{
    public interface ITeacherRepository
    {
        Task<Teacher.Domain.Entities.Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Teacher.Domain.Entities.Teacher>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Teacher.Domain.Entities.Teacher teacher, CancellationToken cancellationToken = default);
        Task UpdateAsync(Teacher.Domain.Entities.Teacher teacher, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> IsPhoneExistsAsync(string phone, CancellationToken cancellationToken = default);
        
        // Assignment related
        Task AddAssignmentAsync(Teacher.Domain.Entities.TeacherClassAssignment assignment, CancellationToken cancellationToken = default);
        Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Teacher.Domain.Entities.TeacherClassAssignment>> GetAssignmentsByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
        
        // ClassInfo local replica (for eventual consistency)
        Task<Teacher.Domain.Entities.ClassInfo?> GetClassInfoByIdAsync(Guid classId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Teacher.Domain.Entities.ClassInfo>> GetClassInfosByIdsAsync(IEnumerable<Guid> classIds, CancellationToken cancellationToken = default);
        Task UpsertClassInfoAsync(Teacher.Domain.Entities.ClassInfo classInfo, CancellationToken cancellationToken = default);
    }
}
