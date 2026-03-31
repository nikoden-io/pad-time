// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Booking.Commands.JoinMatch;

/// <summary>
/// Validates <see cref="JoinMatchCommand"/> ensuring the match identifier and idempotency key are present and valid.
/// </summary>
public sealed class JoinMatchCommandValidator : AbstractValidator<JoinMatchCommand>
{
    public JoinMatchCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty()
            .WithMessage("Match ID is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Idempotency key is required.")
            .MaximumLength(100)
            .WithMessage("Idempotency key is too long.");
    }
}