using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Commands.CreateSite;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.CreateSite;

public class CreateSiteCommandHandlerTests
{
    private readonly ISiteRepository _siteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CreateSiteCommandHandler _handler;
    private readonly DateTime _fixedUtcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public CreateSiteCommandHandlerTests()
    {
        _siteRepository = Substitute.For<ISiteRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _dateTimeProvider.UtcNow.Returns(_fixedUtcNow);

        _handler = new CreateSiteCommandHandler(
            _siteRepository,
            _unitOfWork,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithSiteId()
    {
        // Arrange
        var command = new CreateSiteCommand(
            "Padel Club Brussels",
            "123",
            "Rue de la Loi",
            "1000",
            "Brussels",
            "Belgium",
            "Europe/Brussels");

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAddAsync()
    {
        // Arrange
        var command = new CreateSiteCommand(
            "Test Club",
            "42",
            "Test Street",
            "1000",
            "Brussels",
            "Belgium",
            "Europe/Brussels");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _siteRepository.Received(1).AddAsync(
            Arg.Is<Site>(s =>
                s.Name == "Test Club" &&
                s.City == "Brussels" &&
                s.IsActive == true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var command = new CreateSiteCommand(
            "Test",
            "1",
            "Street",
            "1000",
            "City",
            "Country",
            "Europe/Brussels");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
