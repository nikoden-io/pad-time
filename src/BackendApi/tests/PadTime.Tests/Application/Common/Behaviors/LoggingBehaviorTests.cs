using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PadTime.Application.Common.Behaviors;
using Xunit;

namespace PadTime.Tests.Application.Common.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNextSucceeds_ReturnsResponse()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);

        var result = await behavior.Handle(new TestRequest(), () => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenNextThrows_RethrowsException()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);

        var action = async () => await behavior.Handle(
            new TestRequest(),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    public sealed record TestRequest;
}
