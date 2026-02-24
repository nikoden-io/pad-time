using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Commands.CreateCourt;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.CreateCourt;

public class CreateCourtCommandHandlerTests
{
    private readonly ISiteRepository _siteRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCourtCommandHandler _handler;
    private readonly DateTime _fixedUtcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _siteId = Guid.NewGuid();

    public CreateCourtCommandHandlerTests()
    {
        _siteRepository = Substitute.For<ISiteRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _dateTimeProvider.UtcNow.Returns(_fixedUtcNow);

        _handler = new CreateCourtCommandHandler(
            _siteRepository,
            _dateTimeProvider,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithCourtId()
    {
        // Arrange
        var site = CreateSite();
        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new CreateCourtCommand(site.Id, "Court 1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_SiteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _siteRepository.GetByIdAsync(_siteId, Arg.Any<CancellationToken>())
            .Returns((Site?)null);

        var command = new CreateCourtCommand(_siteId, "Court 1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Site.NotFound.Code);
    }

    [Fact]
    public async Task Handle_DuplicateLabelSameCase_ReturnsDuplicateLabelError()
    {
        // Arrange
        var site = CreateSite();
        site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new CreateCourtCommand(site.Id, "Court 1");

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
        site.AddCourt("Court 1", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new CreateCourtCommand(site.Id, "COURT 1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Court.DuplicateLabel.Code);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var site = CreateSite();
        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var command = new CreateCourtCommand(site.Id, "Court 1");

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
