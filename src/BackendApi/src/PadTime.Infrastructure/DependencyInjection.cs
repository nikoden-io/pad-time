// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;
using PadTime.Infrastructure.BackgroundJobs;
using PadTime.Infrastructure.Persistence;
using PadTime.Infrastructure.Persistence.Repositories;
using PadTime.Infrastructure.Services;

namespace PadTime.Infrastructure;

/// <summary>
/// Registers all infrastructure-layer services including the database context, repositories,
/// unit of work, background jobs, and cross-cutting services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure-layer services to the dependency injection container.
    /// Configures PostgreSQL via Entity Framework Core, registers repositories,
    /// the unit of work, the <see cref="IDateTimeProvider"/>, and the match lifecycle background job.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration for reading the connection string.</param>
    /// <returns>The configured service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<PadTimeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(PadTimeDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(3);
                }));

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PadTimeDbContext>());

        // Repositories
        services.AddScoped<ISiteRepository, SiteRepository>();
        services.AddScoped<ISiteStatisticsRepository, SiteStatisticsRepository>();
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrganizerDebtRepository, OrganizerDebtRepository>();

        // Services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Background jobs
        services.AddHostedService<MatchLifecycleJob>();

        return services;
    }
}