namespace PadTime.Application.Sites.Queries;

/// <summary>
/// DTO representing detailed court information.
/// </summary>
public sealed record CourtDetailDto(
    Guid CourtId,
    string Label,
    bool IsActive,
    DateTime CreatedAtUtc
);

/// <summary>
/// DTO representing site schedule information.
/// </summary>
public sealed record SiteScheduleDto(
    Guid ScheduleId,
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    DayOfWeek[]? ApplicableDays,
    int Priority,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

/// <summary>
/// DTO representing site closure information.
/// </summary>
public sealed record SiteClosureDto(
    Guid ClosureId,
    string Type,
    string Reason,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? ModifiedOpeningTime,
    TimeOnly? ModifiedClosingTime,
    Guid[]? AffectedCourtIds,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
