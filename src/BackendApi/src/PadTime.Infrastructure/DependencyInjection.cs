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

public static class DependencyInjection
{
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
