// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.JoinMatch;

/// <summary>
/// Command for the current user to join a public match. Creates the participation and processes payment atomically.
/// </summary>
/// <param name="MatchId">Unique identifier of the public match to join.</param>
/// <param name="IdempotencyKey">Client-generated key to prevent duplicate join operations.</param>
public sealed record JoinMatchCommand(
    Guid MatchId,
    string IdempotencyKey) : IRequest<Result<JoinMatchResult>>;

/// <summary>
/// Result of joining a match, containing the payment identifier and its status.
/// </summary>
public sealed record JoinMatchResult(
    Guid PaymentId,
    string Status);