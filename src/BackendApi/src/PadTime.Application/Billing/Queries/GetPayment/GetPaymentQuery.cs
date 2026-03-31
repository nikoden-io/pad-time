// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Billing.Queries.GetPayment;

/// <summary>
/// Query to retrieve payment details by identifier. Only the payment owner or an admin can access the data.
/// </summary>
/// <param name="PaymentId">Unique identifier of the payment.</param>
public sealed record GetPaymentQuery(Guid PaymentId) : IRequest<Result<PaymentDto>>;

/// <summary>
/// Data transfer object representing a payment record.
/// </summary>
public sealed record PaymentDto(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    int AmountCents,
    string Status,
    DateTime CreatedAtUtc);