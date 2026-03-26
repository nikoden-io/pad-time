using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Commands.ToggleMemberStatus;

public sealed class ToggleMemberStatusCommandHandler(
    IMemberRepository members,
    IUnitOfWork uow,
    IDateTimeProvider clock)
    : IRequestHandler<ToggleMemberStatusCommand, Result>
{
    public async Task<Result> Handle(ToggleMemberStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
            return DomainErrors.Member.NotFound;

        if (request.IsActive)
            member.Reactivate(clock.UtcNow);
        else
            member.Deactivate(clock.UtcNow);

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
