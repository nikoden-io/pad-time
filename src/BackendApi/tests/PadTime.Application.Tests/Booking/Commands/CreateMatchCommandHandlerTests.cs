using FluentAssertions;
using NSubstitute;
using PadTime.Application.Booking.Commands.CreateMatch;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Application.Tests.Booking.Commands;

public class CreateMatchCommandHandlerTests
{
    private readonly IMatchRepository _matchRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrganizerDebtRepository _debtRepository;
    private readonly IClosureRepository _closureRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateMatchCommandHandler _handler;

    private readonly DateTime _utcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _courtId = Guid.NewGuid();

    public CreateMatchCommandHandlerTests()
    {
        _matchRepository = Substitute.For<IMatchRepository>();
        _siteRepository = Substitute.For<ISiteRepository>();
        _courtRepository = Substitute.For<ICourtRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _debtRepository = Substitute.For<IOrganizerDebtRepository>();
        _closureRepository = Substitute.For<IClosureRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _dateTimeProvider.UtcNow.Returns(_utcNow);
        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(_utcNow));

        _handler = new CreateMatchCommandHandler(
            _matchRepository,
            _siteRepository,
            _courtRepository,
            _memberRepository,
            _debtRepository,
            _closureRepository,
            _currentUser,
            _dateTimeProvider,
            _unitOfWork);
    }

    [Fact]
    public async Task HandleWithValidDataCreatesMatch()
    {
        // Arrange
        var member = SetupMember("G1234");
        SetupSite();
        SetupCourt();
        _closureRepository.IsClosedAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _matchRepository.ExistsForSlotAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateMatchCommand(
            _siteId,
            _courtId,
            _utcNow.AddDays(7),
            PadMatchType.Public);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _matchRepository.Received(1).AddAsync(Arg.Any<Match>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleWithDebtReturnsDebtBlockError()
    {
        // Arrange
        var member = SetupMember("G1234");
        var debt = OrganizerDebt.Create(member.Id, 1500, _utcNow);
        _debtRepository.GetByMemberIdAsync(member.Id, Arg.Any<CancellationToken>())
            .Returns(debt);

        var command = new CreateMatchCommand(
            _siteId,
            _courtId,
            _utcNow.AddDays(7),
            PadMatchType.Public);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Billing.OrganizerDebtBlock.Code);
    }

    [Fact]
    public async Task HandleSlotAlreadyBookedReturnsConflictError()
    {
        // Arrange
        SetupMember("G1234");
        SetupSite();
        SetupCourt();
        _closureRepository.IsClosedAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _matchRepository.ExistsForSlotAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateMatchCommand(
            _siteId,
            _courtId,
            _utcNow.AddDays(7),
            PadMatchType.Public);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Booking.SlotConflict.Code);
    }

    [Fact]
    public async Task HandleSiteMemberWrongSiteReturnsScopeViolation()
    {
        // Arrange
        var memberSiteId = Guid.NewGuid();
        SetupMember("S12345", memberSiteId);
        SetupSite();
        SetupCourt();

        var command = new CreateMatchCommand(
            _siteId, // Different from member's site
            _courtId,
            _utcNow.AddDays(7),
            PadMatchType.Public);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Booking.SiteScopeViolation.Code);
    }

    private Member SetupMember(string matricule, Guid? siteId = null)
    {
        _currentUser.Subject.Returns("sub-123");
        _currentUser.Matricule.Returns(matricule);
        _currentUser.SiteId.Returns(siteId);

        var member = Member.Create("sub-123", matricule, siteId, _utcNow).Value;
        _memberRepository.GetBySubjectAsync("sub-123", Arg.Any<CancellationToken>())
            .Returns(member);
        _debtRepository.GetByMemberIdAsync(member.Id, Arg.Any<CancellationToken>())
            .Returns((OrganizerDebt?)null);

        return member;
    }

    private void SetupSite()
    {
        var site = Site.Create("Test Site", "Europe/Brussels", _utcNow);
        _siteRepository.GetByIdAsync(_siteId, Arg.Any<CancellationToken>())
            .Returns(site);
    }

    private void SetupCourt()
    {
        var site = Site.Create("Test Site", "Europe/Brussels", _utcNow);
        var court = site.AddCourt("Court 1", _utcNow);

        // Use reflection to set the correct IDs
        typeof(Court).GetProperty("Id")!.SetValue(court, _courtId);
        typeof(Court).GetProperty("SiteId")!.SetValue(court, _siteId);

        _courtRepository.GetByIdAsync(_courtId, Arg.Any<CancellationToken>())
            .Returns(court);
    }
}
