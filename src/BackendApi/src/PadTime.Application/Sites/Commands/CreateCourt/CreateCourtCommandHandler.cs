using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateCourt;

public sealed class CreateCourtCommandHandler(
    ISiteRepository siteRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCourtCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateCourtCommand request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        // Check for duplicate label within the site's courts collection
        var duplicateExists = site.Courts.Any(c =>
            string.Equals(c.Label, request.Label, StringComparison.OrdinalIgnoreCase));

        if (duplicateExists)
            return DomainErrors.Court.DuplicateLabel;

        var court = site.AddCourt(request.Label, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return court.Id;
    }
}
