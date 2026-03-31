// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Billing.Commands.PayMatchParticipation;

/// <summary>
/// Command to process a payment for the current user's participation in a match.
/// Uses an idempotency key to prevent duplicate payments.
/// </summary>
/// <param name="MatchId">Unique identifier of the match.</param>
/// <param name="IdempotencyKey">Client-generated key to ensure at-most-once payment processing.</param>
public sealed record PayMatchParticipationCommand(
    Guid MatchId,
    string IdempotencyKey) : IRequest<Result<PayMatchParticipationResult>>;

/// <summary>
/// Result of a match participation payment, containing the payment identifier and its current status.
/// </summary>
public sealed record PayMatchParticipationResult(Guid PaymentId, string Status);