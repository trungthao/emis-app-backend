using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;

namespace Student.Application.UseCases.Students.Queries.GetStudentById;

/// <summary>
/// Query để lấy thông tin học sinh theo Id
/// </summary>
public record GetStudentByIdQuery(Guid StudentId) : IRequest<Result<StudentDto>>;
