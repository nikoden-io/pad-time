using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Commands.ToggleMemberStatus;

public sealed record ToggleMemberStatusCommand(Guid MemberId, bool IsActive) : IRequest<Result>;
