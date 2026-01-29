using FluentAssertions;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Domain.Tests.Members;

public class MatriculeTests
{
    [Theory]
    [InlineData("G1234", MemberCategory.Global)]
    [InlineData("G0001", MemberCategory.Global)]
    [InlineData("G9999", MemberCategory.Global)]
    public void CreateWithValidGlobalMatriculeReturnsGlobalCategory(string value, MemberCategory expectedCategory)
    {
        // Act
        var result = Matricule.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(expectedCategory);
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("S12345", MemberCategory.Site)]
    [InlineData("S00001", MemberCategory.Site)]
    [InlineData("S99999", MemberCategory.Site)]
    public void CreateWithValidSiteMatriculeReturnsSiteCategory(string value, MemberCategory expectedCategory)
    {
        // Act
        var result = Matricule.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData("L12345", MemberCategory.Free)]
    [InlineData("L00001", MemberCategory.Free)]
    [InlineData("L99999", MemberCategory.Free)]
    public void CreateWithValidFreeMatriculeReturnsFreeCategory(string value, MemberCategory expectedCategory)
    {
        // Act
        var result = Matricule.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("G123")]      // Too short
    [InlineData("G12345")]    // Too long
    [InlineData("S1234")]     // Too short
    [InlineData("S123456")]   // Too long
    [InlineData("X12345")]    // Invalid prefix
    [InlineData("g1234")]     // Lowercase (should be normalized to uppercase)
    public void CreateWithInvalidMatriculeReturnsFailure(string value)
    {
        // Act
        var result = Matricule.Create(value);

        // Assert (lowercase should work due to normalization)
        if (value.Equals("G1234", StringComparison.OrdinalIgnoreCase))
        {
            result.IsSuccess.Should().BeTrue();
        }
        else
        {
            result.IsFailure.Should().BeTrue();
        }
    }

    [Fact]
    public void CreateNormalizesToUppercase()
    {
        // Act
        var result = Matricule.Create("g1234");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("G1234");
    }
}
