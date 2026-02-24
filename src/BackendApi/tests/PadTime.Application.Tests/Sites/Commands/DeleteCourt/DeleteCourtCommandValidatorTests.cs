using FluentValidation.TestHelper;
using PadTime.Application.Sites.Commands.DeleteCourt;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.DeleteCourt;

public class DeleteCourtCommandValidatorTests
{
    private readonly DeleteCourtCommandValidator _validator = new();

    [Fact]
    public void Validate_AllPropertiesValid_ReturnsNoErrors()
    {
        // Arrange
        var command = new DeleteCourtCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SiteIdEmpty_ReturnsError()
    {
        // Arrange
        var command = new DeleteCourtCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SiteId);
    }

    [Fact]
    public void Validate_CourtIdEmpty_ReturnsError()
    {
        // Arrange
        var command = new DeleteCourtCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CourtId);
    }
}
