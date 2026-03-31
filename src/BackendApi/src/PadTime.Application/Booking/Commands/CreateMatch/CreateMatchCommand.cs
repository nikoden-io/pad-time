// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.CreateMatch;

/// <summary>
/// Command to create a new padel match on a specific court and time slot.
/// For private matches, initial participants can be specified by matricule.
/// </summary>
/// <param name="SiteId">Identifier of the site where the match is played.</param>
/// <param name="CourtId">Identifier of the court to book.</param>
/// <param name="StartAtUtc">Start time of the match in UTC.</param>
/// <param name="Type">Match type (public or private).</param>
/// <param name="PrivateParticipantMatricules">Optional list of participant matricules for private matches (max 3).</param>
public sealed record CreateMatchCommand(
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    PadMatchType Type,
    List<string>? PrivateParticipantMatricules = null) : IRequest<Result<Guid>>;