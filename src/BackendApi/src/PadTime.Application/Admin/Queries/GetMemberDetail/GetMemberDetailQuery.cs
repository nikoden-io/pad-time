using MediatR;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Application.Admin.Queries.GetMemberDetail;

public sealed record GetMemberDetailQuery(Guid MemberId) : IRequest<Result<MemberDetailDto>>;

public sealed record MemberDetailDto(
    Guid Id,
    string Subject,
    string Matricule,
    MemberCategory Category,
    Guid? SiteId,
    string? SiteName,
    bool IsActive,
    DateTime CreatedAtUtc,
    int MatchCount,
    int DebtAmountCents,
    int TotalMatchesOrganized,
    int TotalMatchesPlayed,
    IReadOnlyList<MemberMatchDto> RecentMatches);

public sealed record MemberMatchDto(
    Guid MatchId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    MatchStatus Status,
    bool IsOrganizer);
