// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using Microsoft.Extensions.Logging;
using PadTime.Application.Common;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking.Events;

namespace PadTime.Application.Billing.EventHandlers;

/// <summary>
/// Handles MatchIncompleteEvent by creating or increasing the organizer's debt.
/// Raised when a match is locked with fewer than 4 paid participants.
/// </summary>
public sealed class MatchIncompleteDebtHandler : INotificationHandler<DomainEventNotification>
{
    private static readonly Action<ILogger, Guid, int, Guid, Exception?> LogDebtIncreased =
        LoggerMessage.Define<Guid, int, Guid>(
            LogLevel.Information,
            new EventId(1, nameof(LogDebtIncreased)),
            "Increased debt for organizer {OrganizerId} by {Amount} cents (match {MatchId})");

    private static readonly Action<ILogger, int, Guid, Guid, Exception?> LogDebtCreated =
        LoggerMessage.Define<int, Guid, Guid>(
            LogLevel.Information,
            new EventId(2, nameof(LogDebtCreated)),
            "Created debt of {Amount} cents for organizer {OrganizerId} (match {MatchId})");

    private readonly IOrganizerDebtRepository _debtRepository;
    private readonly ILogger<MatchIncompleteDebtHandler> _logger;

    public MatchIncompleteDebtHandler(
        IOrganizerDebtRepository debtRepository,
        ILogger<MatchIncompleteDebtHandler> logger)
    {
        _debtRepository = debtRepository;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        if (notification.DomainEvent is not MatchIncompleteEvent evt)
            return;

        var existingDebt = await _debtRepository.GetByMemberIdAsync(evt.OrganizerId, cancellationToken);

        if (existingDebt is not null)
        {
            existingDebt.IncreaseDebt(evt.DebtAmountCents, evt.OccurredOnUtc);
            LogDebtIncreased(_logger, evt.OrganizerId, evt.DebtAmountCents, evt.MatchId, null);
        }
        else
        {
            var debt = OrganizerDebt.Create(evt.OrganizerId, evt.DebtAmountCents, evt.OccurredOnUtc);
            await _debtRepository.AddAsync(debt, cancellationToken);
            LogDebtCreated(_logger, evt.DebtAmountCents, evt.OrganizerId, evt.MatchId, null);
        }
    }
}