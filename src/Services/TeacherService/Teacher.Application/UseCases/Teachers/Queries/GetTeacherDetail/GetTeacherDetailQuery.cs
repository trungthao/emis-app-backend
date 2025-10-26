using MediatR;
using Teacher.Application.DTOs;

namespace Teacher.Application.UseCases.Teachers.Queries.GetTeacherDetail
{
    /// <summary>
    /// Query để lấy chi tiết giáo viên kèm thông tin các lớp học phụ trách
    /// </summary>
    public class GetTeacherDetailQuery : IRequest<TeacherDetailDto>
    {
        public Guid TeacherId { get; set; }
    }
}
