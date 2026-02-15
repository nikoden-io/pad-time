using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteSiteSchedule;

public sealed class DeleteSiteScheduleCommandHandler(
    ISiteRepository siteRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSiteScheduleCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSiteScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdWithSchedulesAndClosuresAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        var result = site.RemoveSchedule(request.ScheduleId, dateTimeProvider.UtcNow);
        if (result.IsFailure)
            return result.PadTimeError;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
