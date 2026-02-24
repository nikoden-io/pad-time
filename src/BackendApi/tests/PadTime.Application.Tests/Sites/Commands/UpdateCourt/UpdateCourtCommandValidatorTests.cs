using FluentValidation.TestHelper;
using PadTime.Application.Sites.Commands.UpdateCourt;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.UpdateCourt;

public class UpdateCourtCommandValidatorTests
{
    private readonly UpdateCourtCommandValidator _validator = new();

    [Fact]
    public void Validate_AllPropertiesValid_ReturnsNoErrors()
    {
        // Arrange
        var command = new UpdateCourtCommand(Guid.NewGuid(), Guid.NewGuid(), "Court 1");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SiteIdEmpty_ReturnsError()
    {
        // Arrange
        var command = new UpdateCourtCommand(Guid.Empty, Guid.NewGuid(), "Court 1");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SiteId);
    }

    [Fact]
    public void Validate_CourtIdEmpty_ReturnsError()
    {
        // Arrange
        var command = new UpdateCourtCommand(Guid.NewGuid(), Guid.Empty, "Court 1");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CourtId);
    }

    [Fact]
    public void Validate_LabelEmpty_ReturnsError()
    {
        // Arrange
        var command = new UpdateCourtCommand(Guid.NewGuid(), Guid.NewGuid(), "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    [Fact]
    public void Validate_LabelExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var longLabel = new string('A', 101);
        var command = new UpdateCourtCommand(Guid.NewGuid(), Guid.NewGuid(), longLabel);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    [Fact]
    public void Validate_LabelWithSpecialCharacters_ReturnsNoErrors()
    {
        // Arrange
        var command = new UpdateCourtCommand(Guid.NewGuid(), Guid.NewGuid(), "Court-1_A");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
