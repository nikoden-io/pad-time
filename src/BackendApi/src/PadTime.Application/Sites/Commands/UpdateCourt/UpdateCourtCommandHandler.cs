using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateCourt;

public sealed class UpdateCourtCommandHandler(
    ISiteRepository siteRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCourtCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCourtCommand request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        // Check for duplicate label within the same site (excluding current court)
        var duplicateExists = site.Courts.Any(c =>
            c.Id != request.CourtId &&
            string.Equals(c.Label, request.Label, StringComparison.OrdinalIgnoreCase));

        if (duplicateExists)
            return DomainErrors.Court.DuplicateLabel;

        var updateResult = site.UpdateCourt(request.CourtId, request.Label, dateTimeProvider.UtcNow);
        if (updateResult.IsFailure)
            return updateResult.PadTimeError;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
