using FluentAssertions;
using FluentValidation.TestHelper;
using PadTime.Application.Sites.Commands.CreateSite;
using Xunit;

namespace PadTime.Application.Tests.Sites.Commands.CreateSite;

public class CreateSiteCommandValidatorTests
{
    private readonly CreateSiteCommandValidator _validator = new();

    [Fact]
    public void Validate_AllPropertiesValid_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateSiteCommand(
            "Valid Name",
            "123",
            "Valid Street",
            "1000",
            "Brussels",
            "Belgium",
            "Europe/Brussels");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameEmpty_ReturnsError()
    {
        // Arrange
        var command = new CreateSiteCommand(
            "",
            "123",
            "Street",
            "1000",
            "City",
            "Country",
            "Europe/Brussels");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var longName = new string('A', 201);
        var command = new CreateSiteCommand(
            longName,
            "123",
            "Street",
            "1000",
            "City",
            "Country",
            "Europe/Brussels");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_AllFieldsEmpty_ReturnsMultipleErrors()
    {
        // Arrange
        var command = new CreateSiteCommand("", "", "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.StreetNumber);
        result.ShouldHaveValidationErrorFor(x => x.Street);
        result.ShouldHaveValidationErrorFor(x => x.Postcode);
        result.ShouldHaveValidationErrorFor(x => x.City);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }
}
