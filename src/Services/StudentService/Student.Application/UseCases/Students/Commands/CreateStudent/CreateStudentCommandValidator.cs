using FluentValidation;

namespace Student.Application.UseCases.Students.Commands.CreateStudent;

/// <summary>
/// Validator cho CreateStudentCommand
/// </summary>
public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.StudentCode)
            .NotEmpty().WithMessage("Mã học sinh không được để trống")
            .MaximumLength(20).WithMessage("Mã học sinh không được quá 20 ký tự");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Tên không được để trống")
            .MaximumLength(50).WithMessage("Tên không được quá 50 ký tự");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Họ không được để trống")
            .MaximumLength(50).WithMessage("Họ không được quá 50 ký tự");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Ngày sinh không được để trống")
            .LessThan(DateTime.Now).WithMessage("Ngày sinh phải nhỏ hơn ngày hiện tại");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Giới tính không hợp lệ");

        RuleFor(x => x.EnrollmentDate)
            .NotEmpty().WithMessage("Ngày nhập học không được để trống");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email không đúng định dạng");

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Số điện thoại phải là 10-11 chữ số");
    }
}
