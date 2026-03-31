// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Billing;

namespace PadTime.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for payment persistence and query operations.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Retrieves a payment by unique identifier, or <c>null</c> if not found.
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a payment by its idempotency key, or <c>null</c> if not found. Used for duplicate detection.
    /// </summary>
    Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all payments for a specific member.
    /// </summary>
    Task<List<Payment>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns paid payments within a date range, optionally filtered by site via their match.
    /// </summary>
    Task<List<(Payment Payment, Guid SiteId)>> GetPaidBySiteAndDateRangeAsync(
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new payment record to the data store.
    /// </summary>
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}