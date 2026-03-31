// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Commands.ToggleMemberStatus;

/// <summary>
/// Handles <see cref="ToggleMemberStatusCommand"/> by activating or deactivating the target member.
/// Returns a domain error when the member does not exist.
/// </summary>
public sealed class ToggleMemberStatusCommandHandler(
    IMemberRepository members,
    IUnitOfWork uow,
    IDateTimeProvider clock)
    : IRequestHandler<ToggleMemberStatusCommand, Result>
{
    /// <inheritdoc />
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