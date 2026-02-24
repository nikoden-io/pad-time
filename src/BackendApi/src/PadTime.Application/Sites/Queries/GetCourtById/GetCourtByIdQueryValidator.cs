using FluentValidation;

namespace PadTime.Application.Sites.Queries.GetCourtById;

public sealed class GetCourtByIdQueryValidator : AbstractValidator<GetCourtByIdQuery>
{
    public GetCourtByIdQueryValidator()
    {
        RuleFor(x => x.CourtId)
            .NotEmpty()
            .WithMessage("Court ID is required");
    }
}