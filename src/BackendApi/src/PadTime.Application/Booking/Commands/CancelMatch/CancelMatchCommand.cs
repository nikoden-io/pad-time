// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.CancelMatch;

/// <summary>
/// Command to cancel a match. The organizer can cancel non-locked matches; admins can cancel any match within their scope.
/// </summary>
/// <param name="MatchId">Unique identifier of the match to cancel.</param>
public sealed record CancelMatchCommand(Guid MatchId) : IRequest<Result>;