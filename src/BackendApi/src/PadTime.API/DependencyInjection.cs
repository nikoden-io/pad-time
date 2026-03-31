using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PadTime.API.Authorization;
using PadTime.API.Authorization.Handlers;
using PadTime.API.Authorization.Requirements;
using PadTime.API.Services;
using PadTime.Application.Common.Interfaces;

namespace PadTime.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.ModelValidatorProviders.Clear();
        });

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PadTime API",
                Version = "v1",
                Description = "Padel court booking platform API"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var authEnabled = configuration.GetValue<bool>("Authentication:Enabled", true);

        if (authEnabled)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var authority = configuration["Authentication:Authority"] ?? "https://localhost:5001";
                    var requireHttpsMetadata = configuration.GetValue<bool>("Authentication:RequireHttpsMetadata", true);

                    options.Authority = authority;
                    options.Audience = configuration["Authentication:Audience"];
                    options.RequireHttpsMetadata = requireHttpsMetadata;

                    if (!requireHttpsMetadata)
                    {
                        options.BackchannelHttpHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        ValidIssuers = new[]
                        {
                            authority,
                            "https://localhost:5001",
                            "https://identity-server:443"
                        }
                    };
                });

            services.AddScoped<IAuthorizationHandler, SiteAccessHandler>();
            services.AddScoped<IAuthorizationHandler, SiteManagementHandler>();
        }
        else
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        }

        services.AddAuthorization(options =>
        {
            if (authEnabled)
            {
                options.AddPolicy(Policies.RequireUser, policy =>
                    policy.RequireAuthenticatedUser());

                options.AddPolicy(Policies.RequireAdmin, policy =>
                    policy.RequireRole("admin_site", "admin_global"));

                options.AddPolicy(Policies.RequireGlobalAdmin, policy =>
                    policy.RequireRole("admin_global"));

                options.AddPolicy(Policies.RequireSiteAdmin, policy =>
                    policy.RequireRole("admin_site", "admin_global"));

                options.AddPolicy(Policies.RequireSiteAccess, policy =>
                    policy.Requirements.Add(new SiteAccessRequirement()));

                options.AddPolicy(Policies.RequireSiteManagement, policy =>
                    policy.Requirements.Add(new SiteManagementRequirement()));
            }
            else
            {
                options.AddPolicy(Policies.RequireUser, policy =>
                    policy.RequireAssertion(_ => true));
                options.AddPolicy(Policies.RequireAdmin, policy =>
                    policy.RequireAssertion(_ => true));
                options.AddPolicy(Policies.RequireGlobalAdmin, policy =>
                    policy.RequireAssertion(_ => true));
                options.AddPolicy(Policies.RequireSiteAdmin, policy =>
                    policy.RequireAssertion(_ => true));
                options.AddPolicy(Policies.RequireSiteAccess, policy =>
                    policy.RequireAssertion(_ => true));
                options.AddPolicy(Policies.RequireSiteManagement, policy =>
                    policy.RequireAssertion(_ => true));

                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            }
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                policy
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        // Health checks
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "database",
                tags: ["ready"]);

        return services;
    }
}
