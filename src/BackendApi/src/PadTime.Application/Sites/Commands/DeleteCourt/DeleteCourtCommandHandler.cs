using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteCourt;

public sealed class DeleteCourtCommandHandler(
    ISiteRepository siteRepository,
    IMatchRepository matchRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCourtCommand, Result>
{
    public async Task<Result> Handle(
        DeleteCourtCommand request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        var court = site.Courts.FirstOrDefault(c => c.Id == request.CourtId);
        if (court is null)
            return DomainErrors.Court.NotFound;

        // Check for active or future bookings
        var hasActiveBookings = await matchRepository.HasActiveBookingsForCourtAsync(request.CourtId, cancellationToken);
        if (hasActiveBookings)
            return DomainErrors.Court.CannotDeleteWithActiveBookings;

        var removeResult = site.RemoveCourt(request.CourtId, dateTimeProvider.UtcNow);
        if (removeResult.IsFailure)
            return removeResult.PadTimeError;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
