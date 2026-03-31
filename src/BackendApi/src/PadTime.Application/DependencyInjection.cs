// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PadTime.Application.Common.Behaviors;

namespace PadTime.Application;

/// <summary>
/// Registers application-layer services (MediatR, FluentValidation, pipeline behaviors) into the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all application-layer services to the service collection.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EnsureMemberExistsBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}