// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Infrastructure.Services;

/// <summary>
/// Production implementation of <see cref="IDateTimeProvider"/> that returns the real system clock values.
/// Registered as a singleton to allow substitution with a test double in integration tests.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}