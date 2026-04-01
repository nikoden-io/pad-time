using FluentAssertions;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Domain.Members;

public sealed class MatriculeTests
{
    [Theory]
    [InlineData("g1234", "G1234", MemberCategory.Global)]
    [InlineData("s12345", "S12345", MemberCategory.Site)]
    [InlineData("l12345", "L12345", MemberCategory.Free)]
    public void Create_WithValidValue_NormalizesAndDerivesCategory(
        string rawValue,
        string expectedValue,
        MemberCategory expectedCategory)
    {
        var result = Matricule.Create(rawValue);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expectedValue);
        result.Value.Category.Should().Be(expectedCategory);
        result.Value.ToString().Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("X1234")]
    [InlineData("G12345")]
    [InlineData("S1234")]
    public void Create_WithInvalidValue_ReturnsInvalidMatricule(string rawValue)
    {
        var result = Matricule.Create(rawValue);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Member.InvalidMatricule);
    }
}
