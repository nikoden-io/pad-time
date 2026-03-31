// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetMatch;

/// <summary>
/// Query to retrieve detailed information about a specific match, including participants.
/// Private matches are only visible to participants and admins.
/// </summary>
/// <param name="MatchId">Unique identifier of the match.</param>
public sealed record GetMatchQuery(Guid MatchId) : IRequest<Result<MatchDto>>;

/// <summary>
/// Detailed match information including court, schedule, pricing, and participant list.
/// </summary>
public sealed record MatchDto(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Type,
    string Status,
    Guid OrganizerId,
    int PriceTotalCents,
    IReadOnlyList<ParticipantDto> Participants);

/// <summary>
/// Summary of a match participant including their role and payment status.
/// </summary>
public sealed record ParticipantDto(
    Guid MemberId,
    string Matricule,
    string Role,
    string PaymentStatus);