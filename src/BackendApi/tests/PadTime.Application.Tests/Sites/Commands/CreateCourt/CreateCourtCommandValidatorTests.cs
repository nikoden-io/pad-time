using FluentValidation.TestHelper;
using PadTime.Application.Sites.Commands.CreateCourt;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.CreateCourt;

public class CreateCourtCommandValidatorTests
{
    private readonly CreateCourtCommandValidator _validator = new();

    [Fact]
    public void Validate_AllPropertiesValid_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateCourtCommand(Guid.NewGuid(), "Court 1");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_LabelEmpty_ReturnsError()
    {
        // Arrange
        var command = new CreateCourtCommand(Guid.NewGuid(), "");

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
        var command = new CreateCourtCommand(Guid.NewGuid(), longLabel);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    [Fact]
    public void Validate_SiteIdEmpty_ReturnsError()
    {
        // Arrange
        var command = new CreateCourtCommand(Guid.Empty, "Court 1");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SiteId);
    }
}
