using FluentAssertions;
using FluentValidation;
using PadTime.Application.Common.Behaviors;
using Xunit;

namespace PadTime.Tests.Application.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNoValidatorsConfigured_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);

        var result = await behavior.Handle(new TestRequest("ok"), () => Task.FromResult("next"), CancellationToken.None);

        result.Should().Be("next");
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new TestRequestValidator()]);

        var action = async () => await behavior.Handle(
            new TestRequest(string.Empty),
            () => Task.FromResult("next"),
            CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    private sealed record TestRequest(string Name);

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
