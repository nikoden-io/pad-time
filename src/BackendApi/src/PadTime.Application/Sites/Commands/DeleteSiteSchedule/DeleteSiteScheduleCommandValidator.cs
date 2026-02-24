using FluentValidation;

namespace PadTime.Application.Sites.Commands.DeleteSiteSchedule;

public sealed class DeleteSiteScheduleCommandValidator : AbstractValidator<DeleteSiteScheduleCommand>
{
    public DeleteSiteScheduleCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID is required.");
    }
}
