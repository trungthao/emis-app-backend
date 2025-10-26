using MediatR;
using Microsoft.Extensions.Logging;
using Teacher.Application.DTOs;
using Teacher.Domain.Repositories;

namespace Teacher.Application.UseCases.Teachers.Queries.GetTeacherDetail
{
    public class GetTeacherDetailQueryHandler : IRequestHandler<GetTeacherDetailQuery, TeacherDetailDto>
    {
        private readonly ITeacherRepository _repository;
        private readonly ILogger<GetTeacherDetailQueryHandler> _logger;

        public GetTeacherDetailQueryHandler(
            ITeacherRepository repository,
            ILogger<GetTeacherDetailQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TeacherDetailDto> Handle(GetTeacherDetailQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting teacher detail for TeacherId: {TeacherId}", request.TeacherId);

            // Lấy thông tin giáo viên
            var teacher = await _repository.GetByIdAsync(request.TeacherId, cancellationToken);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher not found: {TeacherId}", request.TeacherId);
                throw new InvalidOperationException($"Teacher with ID {request.TeacherId} not found");
            }

            // Lấy danh sách assignments
            var assignments = await _repository.GetAssignmentsByTeacherIdAsync(request.TeacherId, cancellationToken);
            var assignmentList = assignments.ToList();

            // PRODUCTION-READY: Batch load class info from local replica (1 query instead of N queries)
            var classIds = assignmentList.Select(a => a.ClassId).Distinct().ToList();
            var classInfos = await _repository.GetClassInfosByIdsAsync(classIds, cancellationToken);
            var classInfoDict = classInfos.ToDictionary(c => c.ClassId);

            // Map sang DTO kèm thông tin lớp học
            var assignmentDetailDtos = new List<TeacherAssignmentDetailDto>();
            
            foreach (var assignment in assignmentList)
            {
                // Get class info from local replica (eventual consistency pattern)
                // Data synchronized via ClassCreated/ClassUpdated events
                var classInfo = classInfoDict.GetValueOrDefault(assignment.ClassId);
                
                assignmentDetailDtos.Add(new TeacherAssignmentDetailDto
                {
                    AssignmentId = assignment.Id,
                    TeacherId = assignment.TeacherId,
                    ClassId = assignment.ClassId,
                    Role = assignment.Role,
                    AssignedAt = assignment.AssignedAt,
                    ClassName = classInfo?.ClassName ?? "Unknown Class",
                    Grade = classInfo?.Grade,
                    AcademicYear = classInfo?.AcademicYear,
                    TotalStudents = classInfo?.TotalStudents
                });
            }

            // Map sang TeacherDetailDto
            var result = new TeacherDetailDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                FullName = teacher.FullName,
                DateOfBirth = teacher.DateOfBirth,
                Phone = teacher.Phone,
                Email = teacher.Email,
                Address = teacher.Address,
                AvatarUrl = teacher.AvatarUrl,
                Status = teacher.Status.ToString(),
                CreatedAt = teacher.CreatedAt,
                UpdatedAt = teacher.UpdatedAt,
                ClassAssignments = assignmentDetailDtos,
                TotalClassesAssigned = assignmentDetailDtos.Count
            };

            _logger.LogInformation(
                "Retrieved teacher detail for {FullName} with {ClassCount} class assignments",
                teacher.FullName,
                assignmentDetailDtos.Count);

            return result;
        }
    }
}
