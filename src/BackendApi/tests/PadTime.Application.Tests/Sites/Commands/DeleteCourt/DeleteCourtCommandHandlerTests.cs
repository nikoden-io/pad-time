using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Commands.DeleteCourt;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.DeleteCourt;

public class DeleteCourtCommandHandlerTests
{
    private readonly ISiteRepository _siteRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteCourtCommandHandler _handler;
    private readonly DateTime _fixedUtcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _courtId = Guid.NewGuid();

    public DeleteCourtCommandHandlerTests()
    {
        _siteRepository = Substitute.For<ISiteRepository>();
        _matchRepository = Substitute.For<IMatchRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _dateTimeProvider.UtcNow.Returns(_fixedUtcNow);

        _handler = new DeleteCourtCommandHandler(
            _siteRepository,
            _matchRepository,
            _dateTimeProvider,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var site = CreateSite();
        var court = site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);
        _matchRepository.HasActiveBookingsForCourtAsync(court.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteCourtCommand(site.Id, court.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SiteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _siteRepository.GetByIdAsync(_siteId, Arg.Any<CancellationToken>())
            .Returns((Site?)null);

        var command = new DeleteCourtCommand(_siteId, _courtId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Site.NotFound.Code);
    }

    [Fact]
    public async Task Handle_CourtNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var site = CreateSite();
        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new DeleteCourtCommand(site.Id, _courtId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Court.NotFound.Code);
    }

    [Fact]
    public async Task Handle_CourtHasActiveBookings_ReturnsCannotDeleteError()
    {
        // Arrange
        var site = CreateSite();
        var court = site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);
        _matchRepository.HasActiveBookingsForCourtAsync(court.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteCourtCommand(site.Id, court.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Court.CannotDeleteWithActiveBookings.Code);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var site = CreateSite();
        var court = site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);
        _matchRepository.HasActiveBookingsForCourtAsync(court.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteCourtCommand(site.Id, court.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksForActiveBookings()
    {
        // Arrange
        var site = CreateSite();
        var court = site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);
        _matchRepository.HasActiveBookingsForCourtAsync(court.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteCourtCommand(site.Id, court.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _matchRepository.Received(1).HasActiveBookingsForCourtAsync(court.Id, Arg.Any<CancellationToken>());
    }

    private Site CreateSite()
    {
        return Site.Create(
            "Test Site",
            "123",
            "Test Street",
            "1000",
            "Brussels",
            "Belgium",
            "Europe/Brussels",
            _fixedUtcNow);
    }
}
