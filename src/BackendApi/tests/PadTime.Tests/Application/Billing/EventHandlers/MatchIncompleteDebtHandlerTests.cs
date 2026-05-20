using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PadTime.Application.Billing.EventHandlers;
using PadTime.Application.Common;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking.Events;
using Xunit;

namespace PadTime.Tests.Application.Billing.EventHandlers;

public sealed class MatchIncompleteDebtHandlerTests
{
    [Fact]
    public async Task Handle_WhenDebtExists_IncreasesExistingDebt()
    {
        var debtRepository = Substitute.For<IOrganizerDebtRepository>();
        var logger = Substitute.For<ILogger<MatchIncompleteDebtHandler>>();
        var handler = new MatchIncompleteDebtHandler(debtRepository, logger);
        var organizerId = Guid.NewGuid();
        var debt = OrganizerDebt.Create(organizerId, 1500, DateTime.UtcNow);

        debtRepository.GetByMemberIdAsync(organizerId, Arg.Any<CancellationToken>()).Returns(debt);

        await handler.Handle(
            new DomainEventNotification(new MatchIncompleteEvent(Guid.NewGuid(), organizerId, 3000, DateTime.UtcNow)),
            CancellationToken.None);

        debt.AmountCents.Should().Be(4500);
        await debtRepository.DidNotReceive().AddAsync(Arg.Any<OrganizerDebt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDebtDoesNotExist_CreatesDebt()
    {
        var debtRepository = Substitute.For<IOrganizerDebtRepository>();
        var logger = Substitute.For<ILogger<MatchIncompleteDebtHandler>>();
        var handler = new MatchIncompleteDebtHandler(debtRepository, logger);
        var organizerId = Guid.NewGuid();

        debtRepository.GetByMemberIdAsync(organizerId, Arg.Any<CancellationToken>()).Returns((OrganizerDebt?)null);

        await handler.Handle(
            new DomainEventNotification(new MatchIncompleteEvent(Guid.NewGuid(), organizerId, 3000, DateTime.UtcNow)),
            CancellationToken.None);

        await debtRepository.Received(1).AddAsync(
            Arg.Is<OrganizerDebt>(d => d.MemberId == organizerId && d.AmountCents == 3000),
            Arg.Any<CancellationToken>());
    }
}
