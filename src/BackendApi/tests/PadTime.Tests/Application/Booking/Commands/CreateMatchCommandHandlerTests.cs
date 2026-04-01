using FluentAssertions;
using NSubstitute;
using PadTime.Application.Booking.Commands.CreateMatch;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using PadTime.Domain.Site;
using PadTime.Tests.TestSupport;
using Xunit;

namespace PadTime.Tests.Application.Booking.Commands;

public sealed class CreateMatchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAllRulesPass_CreatesMatchAndPersistsChanges()
    {
        var siteId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userMember = Member.Create("subject-1", "G1234", null, DateTime.UtcNow).Value;
        var currentUser = CreateCurrentUser("subject-1", "G1234");
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        dateTimeProvider.Today.Returns(new DateOnly(2026, 4, 1));

        var matchRepository = Substitute.For<IMatchRepository>();
        var siteRepository = Substitute.For<ISiteRepository>();
        var courtRepository = Substitute.For<ICourtRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var debtRepository = Substitute.For<IOrganizerDebtRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        memberRepository.GetBySubjectAsync(currentUser.Subject, Arg.Any<CancellationToken>())
            .Returns(userMember);
        debtRepository.GetByMemberIdAsync(userMember.Id, Arg.Any<CancellationToken>())
            .Returns((OrganizerDebt?)null);
        siteRepository.GetByIdWithSchedulesAndClosuresAsync(siteId, Arg.Any<CancellationToken>())
            .Returns(CreateSiteWithCourtAndSchedule(siteId, courtId));
        courtRepository.GetByIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(CreateCourt(siteId, courtId));
        matchRepository.ExistsForSlotAsync(courtId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new CreateMatchCommandHandler(
            matchRepository,
            siteRepository,
            courtRepository,
            memberRepository,
            debtRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var command = new CreateMatchCommand(
            siteId,
            courtId,
            new DateTime(2026, 4, 5, 9, 0, 0, DateTimeKind.Utc),
            PadMatchType.Public);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await matchRepository.Received(1).AddAsync(
            Arg.Is<Match>(m =>
                m.SiteId == siteId &&
                m.CourtId == courtId &&
                m.OrganizerId == userMember.Id &&
                m.Status == MatchStatus.Public),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserHasDebt_ReturnsOrganizerDebtBlock()
    {
        var userMember = Member.Create("subject-1", "G1234", null, DateTime.UtcNow).Value;
        var currentUser = CreateCurrentUser("subject-1", "G1234");
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        dateTimeProvider.Today.Returns(new DateOnly(2026, 4, 1));

        var matchRepository = Substitute.For<IMatchRepository>();
        var siteRepository = Substitute.For<ISiteRepository>();
        var courtRepository = Substitute.For<ICourtRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var debtRepository = Substitute.For<IOrganizerDebtRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        memberRepository.GetBySubjectAsync(currentUser.Subject, Arg.Any<CancellationToken>())
            .Returns(userMember);
        debtRepository.GetByMemberIdAsync(userMember.Id, Arg.Any<CancellationToken>())
            .Returns(OrganizerDebt.Create(userMember.Id, 1500, DateTime.UtcNow));

        var handler = new CreateMatchCommandHandler(
            matchRepository,
            siteRepository,
            courtRepository,
            memberRepository,
            debtRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(
            new CreateMatchCommand(Guid.NewGuid(), Guid.NewGuid(), new DateTime(2026, 4, 5, 9, 0, 0, DateTimeKind.Utc), PadMatchType.Public),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Billing.OrganizerDebtBlock);
        await matchRepository.DidNotReceive().AddAsync(Arg.Any<Match>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPrivateParticipantMatriculeDoesNotExist_ReturnsMemberNotFound()
    {
        var siteId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userMember = Member.Create("subject-1", "G1234", null, DateTime.UtcNow).Value;
        var currentUser = CreateCurrentUser("subject-1", "G1234");
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        dateTimeProvider.Today.Returns(new DateOnly(2026, 4, 1));

        var matchRepository = Substitute.For<IMatchRepository>();
        var siteRepository = Substitute.For<ISiteRepository>();
        var courtRepository = Substitute.For<ICourtRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var debtRepository = Substitute.For<IOrganizerDebtRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        memberRepository.GetBySubjectAsync(currentUser.Subject, Arg.Any<CancellationToken>())
            .Returns(userMember);
        debtRepository.GetByMemberIdAsync(userMember.Id, Arg.Any<CancellationToken>())
            .Returns((OrganizerDebt?)null);
        siteRepository.GetByIdWithSchedulesAndClosuresAsync(siteId, Arg.Any<CancellationToken>())
            .Returns(CreateSiteWithCourtAndSchedule(siteId, courtId));
        courtRepository.GetByIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(CreateCourt(siteId, courtId));
        matchRepository.ExistsForSlotAsync(courtId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);
        memberRepository.GetByMatriculeAsync("S12345", Arg.Any<CancellationToken>())
            .Returns((Member?)null);

        var handler = new CreateMatchCommandHandler(
            matchRepository,
            siteRepository,
            courtRepository,
            memberRepository,
            debtRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(
            new CreateMatchCommand(siteId, courtId, new DateTime(2026, 4, 5, 9, 0, 0, DateTimeKind.Utc), PadMatchType.Private, ["S12345"]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Member.NotFound);
        await matchRepository.DidNotReceive().AddAsync(Arg.Any<Match>(), Arg.Any<CancellationToken>());
    }

    private static ICurrentUser CreateCurrentUser(string subject, string matricule, Guid? siteId = null)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Subject.Returns(subject);
        currentUser.Matricule.Returns(matricule);
        currentUser.SiteId.Returns(siteId);
        currentUser.IsAuthenticated.Returns(true);
        currentUser.IsAdmin.Returns(false);
        currentUser.IsSiteAdmin.Returns(false);
        currentUser.IsGlobalAdmin.Returns(false);
        return currentUser;
    }

    private static Site CreateSiteWithCourtAndSchedule(Guid siteId, Guid courtId)
    {
        var site = Site.Create("Site", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.SetEntityId(siteId);
        var court = site.AddCourt("Court 1", DateTime.UtcNow);
        court.SetEntityId(courtId);
        site.AddSchedule(
            "Standard",
            new DateOnly(2026, 1, 1),
            null,
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            null,
            1,
            DateTime.UtcNow);
        return site;
    }

    private static Court CreateCourt(Guid siteId, Guid courtId)
    {
        var site = Site.Create("Site", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.SetEntityId(siteId);
        var court = site.AddCourt("Court 1", DateTime.UtcNow);
        court.SetEntityId(courtId);
        return court;
    }
}
