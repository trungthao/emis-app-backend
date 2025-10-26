using FluentValidation;

namespace Message.Application.Commands;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId không được để trống");

        RuleFor(x => x.SenderId)
            .NotEmpty()
            .WithMessage("SenderId không được để trống");

        RuleFor(x => x.SenderName)
            .NotEmpty()
            .WithMessage("SenderName không được để trống");

        RuleFor(x => x.Content)
            .NotEmpty()
            .When(x => x.Attachments == null || x.Attachments.Count == 0)
            .WithMessage("Tin nhắn phải có nội dung hoặc file đính kèm");

        RuleFor(x => x.Content)
            .MaximumLength(5000)
            .WithMessage("Nội dung tin nhắn không được vượt quá 5000 ký tự");
    }
}
