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

        // Behaviors are registered as open generics (IPipelineBehavior<,>),
        // so the descriptors carry the open generic service/implementation types.
        var descriptors = services
            .Where(s => s.ServiceType == typeof(IPipelineBehavior<,>))
            .ToList();

        descriptors.Should().ContainSingle(d => d.ImplementationType == typeof(LoggingBehavior<,>));
        descriptors.Should().ContainSingle(d => d.ImplementationType == typeof(EnsureMemberExistsBehavior<,>));
        descriptors.Should().ContainSingle(d => d.ImplementationType == typeof(ValidationBehavior<,>));
    }
}
