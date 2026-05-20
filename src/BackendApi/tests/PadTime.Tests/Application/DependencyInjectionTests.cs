using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PadTime.Application;
using PadTime.Application.Common.Behaviors;
using Xunit;

namespace PadTime.Tests.Application;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_WhenCalled_RegistersPipelineBehaviors()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptors = services
            .Where(s => s.ServiceType == typeof(IPipelineBehavior<DummyRequest, DummyResponse>))
            .ToList();

        descriptors.Should().ContainSingle(d => d.ImplementationType == typeof(LoggingBehavior<DummyRequest, DummyResponse>));
        descriptors.Should().ContainSingle(d => d.ImplementationType == typeof(EnsureMemberExistsBehavior<DummyRequest, DummyResponse>));
        descriptors.Should().ContainSingle(d => d.ImplementationType == typeof(ValidationBehavior<DummyRequest, DummyResponse>));
    }

    private sealed record DummyRequest : IRequest<DummyResponse>;

    private sealed record DummyResponse;
}
