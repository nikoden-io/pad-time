using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Commands.UpdateSite;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.UpdateSite;

public class UpdateSiteCommandHandlerTests
{
    private readonly ISiteRepository _siteRepository = Substitute.For<ISiteRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdateSiteCommandHandler _handler;

    public UpdateSiteCommandHandlerTests()
    {
        _handler = new UpdateSiteCommandHandler(_siteRepository, _unitOfWork, _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_SiteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new UpdateSiteCommand(
            Guid.NewGuid(),
            "Updated Site",
            "456",
            "Updated Street",
            "2000",
            "Updated City",
            "Updated Country",
            "Europe/Brussels");

        _siteRepository.GetByIdAsync(command.SiteId, Arg.Any<CancellationToken>())
            .Returns((Site?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task Handle_ValidSite_UpdatesSuccessfully()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;
        
        var existingSite = Site.Create(
            "Original Site",
            "123",
            "Original Street",
            "1000",
            "Original City",
            "Original Country",
            "Europe/Brussels",
            utcNow);

        var command = new UpdateSiteCommand(
            siteId,
            "Updated Site",
            "456",
            "Updated Street",
            "2000",
            "Updated City",
            "Updated Country",
            "Europe/Paris");

        _siteRepository.GetByIdAsync(siteId, Arg.Any<CancellationToken>())
            .Returns(existingSite);
        _dateTimeProvider.UtcNow.Returns(utcNow.AddHours(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify site was updated
        existingSite.Name.Should().Be("Updated Site");
        existingSite.StreetNumber.Should().Be("456");
        existingSite.Street.Should().Be("Updated Street");
        existingSite.Postcode.Should().Be("2000");
        existingSite.City.Should().Be("Updated City");
        existingSite.Country.Should().Be("Updated Country");
        existingSite.Timezone.Should().Be("Europe/Paris");

        // Verify repository calls
        await _siteRepository.Received(1).UpdateAsync(existingSite, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}