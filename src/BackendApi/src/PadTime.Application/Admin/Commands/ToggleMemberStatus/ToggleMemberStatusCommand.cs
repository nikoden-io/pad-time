// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Commands.ToggleMemberStatus;

/// <summary>
/// Command to activate or deactivate a member account.
/// </summary>
/// <param name="MemberId">Unique identifier of the member to update.</param>
/// <param name="IsActive">When <c>true</c> the member is reactivated; when <c>false</c> the member is deactivated.</param>
public sealed record ToggleMemberStatusCommand(Guid MemberId, bool IsActive) : IRequest<Result>;