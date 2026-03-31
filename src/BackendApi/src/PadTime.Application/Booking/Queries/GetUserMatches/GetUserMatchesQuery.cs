// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetUserMatches;

/// <summary>
/// Query to retrieve matches where the current authenticated user is a participant.
/// Includes matches organized by the user and matches joined as a participant.
/// Supports optional date filtering and pagination.
/// </summary>
/// <param name="FromUtc">
/// Optional UTC date filter. When provided, only matches starting on or after this date are returned.
/// </param>
/// <param name="Page">
/// Page number for pagination. Starts at 1.
/// </param>
/// <param name="PageSize">
/// Number of matches per page.
/// </param>
public sealed record GetUserMatchesQuery(
    DateTime? FromUtc,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<UserMatchDto>>>;

/// <summary>
/// Data transfer object representing a match visible to the current user.
/// </summary>
/// <param name="MatchId">Unique identifier of the match.</param>
/// <param name="SiteId">Identifier of the site where the match takes place.</param>
/// <param name="CourtId">Identifier of the court.</param>
/// <param name="StartAtUtc">Match start time (UTC).</param>
/// <param name="EndAtUtc">Match end time (UTC).</param>
/// <param name="Type">Match type (public or private).</param>
/// <param name="Status">Current match status.</param>
/// <param name="OrganizerId">Identifier of the organizer.</param>
/// <param name="PriceTotalCents">Total price of the match in cents.</param>
/// <param name="Participants">List of match participants.</param>
public sealed record UserMatchDto(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Type,
    string Status,
    Guid OrganizerId,
    int PriceTotalCents,
    IReadOnlyList<UserParticipantDto> Participants);

/// <summary>
/// Data transfer object representing a participant in a match.
/// </summary>
/// <param name="MemberId">Identifier of the member.</param>
/// <param name="Matricule">Member matricule.</param>
/// <param name="Role">Participant role in the match.</param>
/// <param name="PaymentStatus">Payment status of the participant.</param>
public sealed record UserParticipantDto(
    Guid MemberId,
    string Matricule,
    string Role,
    string PaymentStatus);