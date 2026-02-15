using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Commands.UpdateCourt;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.UpdateCourt;

public class UpdateCourtCommandHandlerTests
{
    private readonly ISiteRepository _siteRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCourtCommandHandler _handler;
    private readonly DateTime _fixedUtcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _courtId = Guid.NewGuid();

    public UpdateCourtCommandHandlerTests()
    {
        _siteRepository = Substitute.For<ISiteRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _dateTimeProvider.UtcNow.Returns(_fixedUtcNow);

        _handler = new UpdateCourtCommandHandler(
            _siteRepository,
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

        var command = new UpdateCourtCommand(site.Id, court.Id, "Updated Court");

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

        var command = new UpdateCourtCommand(_siteId, _courtId, "Updated Court");

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

        var command = new UpdateCourtCommand(site.Id, _courtId, "Updated Court");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Court.NotFound.Code);
    }

    [Fact]
    public async Task Handle_DuplicateLabelSameCase_ReturnsDuplicateLabelError()
    {
        // Arrange
        var site = CreateSite();
        var court1 = site.AddCourt("Court 1", _fixedUtcNow);
        var court2 = site.AddCourt("Court 2", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new UpdateCourtCommand(site.Id, court1.Id, "Court 2");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Court.DuplicateLabel.Code);
    }

    [Fact]
    public async Task Handle_DuplicateLabelDifferentCase_ReturnsDuplicateLabelError()
    {
        // Arrange
        var site = CreateSite();
        var court1 = site.AddCourt("Court 1", _fixedUtcNow);
        var court2 = site.AddCourt("Court 2", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new UpdateCourtCommand(site.Id, court1.Id, "COURT 2");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Court.DuplicateLabel.Code);
    }

    [Fact]
    public async Task Handle_SameLabelAsCurrentCourt_ReturnsSuccess()
    {
        // Arrange
        var site = CreateSite();
        var court = site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new UpdateCourtCommand(site.Id, court.Id, "Court 1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var site = CreateSite();
        var court = site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new UpdateCourtCommand(site.Id, court.Id, "Updated Court");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
