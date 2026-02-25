using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PadTime.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace PadTime.API.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("padtime_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PadTimeDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database (Testcontainers)
            services.AddDbContext<PadTimeDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Bypass authentication in tests
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });

        builder.UseEnvironment("Test");
    }

    // FIX : Serilog est initialisé dans Program.cs via UseSerilog() bootstrapping.
    // Quand WebApplicationFactory reconstruit l'host, Serilog est déjà "frozen"
    // et lève InvalidOperationException.
    // Solution : on surcharge CreateHostBuilder pour retirer Serilog avant que
    // WebApplicationFactory ne reconstruise l'app, et on le remplace par le
    // logging standard .NET (suffisant pour les tests).
    protected override IHostBuilder CreateHostBuilder()
    {
        var builder = base.CreateHostBuilder()
            ?? Host.CreateDefaultBuilder();

        builder.ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PadTimeDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "admin_global"),
            new Claim("sub", Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
