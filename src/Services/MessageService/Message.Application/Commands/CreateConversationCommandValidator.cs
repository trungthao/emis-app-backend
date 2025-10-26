using FluentValidation;
using Message.Domain.Enums;

namespace Message.Application.Commands;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Loại conversation không hợp lệ");

        RuleFor(x => x.GroupName)
            .NotEmpty()
            .When(x => x.Type == ConversationType.CustomGroup)
            .WithMessage("Tên nhóm không được để trống");

        RuleFor(x => x.StudentId)
            .NotEmpty()
            .When(x => x.Type == ConversationType.StudentGroup)
            .WithMessage("StudentId không được để trống khi tạo StudentGroup");

        RuleFor(x => x.MemberIds)
            .NotEmpty()
            .WithMessage("Phải có ít nhất 1 thành viên")
            .Must(x => x.Count >= 2)
            .When(x => x.Type == ConversationType.DirectMessage)
            .WithMessage("Direct message phải có đúng 2 thành viên");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("CreatedBy không được để trống");
    }
}
