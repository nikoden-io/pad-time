// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.AddParticipant;

/// <summary>
/// Command to add a participant to a match by their matricule. Only the match organizer can perform this action.
/// </summary>
/// <param name="MatchId">Unique identifier of the match.</param>
/// <param name="Matricule">Business identifier (matricule) of the member to add.</param>
public sealed record AddParticipantCommand(Guid MatchId, string Matricule) : IRequest<Result>;