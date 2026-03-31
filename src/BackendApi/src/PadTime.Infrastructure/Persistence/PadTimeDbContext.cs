// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common;
using PadTime.Application.Common.Interfaces;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the PadTime application.
/// Implements <see cref="IUnitOfWork"/> to coordinate transactional persistence
/// and dispatches domain events before saving changes.
/// </summary>
public sealed class PadTimeDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PadTimeDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options including the connection string.</param>
    /// <param name="mediator">MediatR mediator used to dispatch domain events before persisting.</param>
    public PadTimeDbContext(DbContextOptions<PadTimeDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    /// <summary>Gets the set of matches (bookings).</summary>
    public DbSet<Match> Matches => Set<Match>();

    /// <summary>Gets the set of match participants.</summary>
    public DbSet<Participant> Participants => Set<Participant>();

    /// <summary>Gets the set of club members.</summary>
    public DbSet<Member> Members => Set<Member>();

    /// <summary>Gets the set of payments.</summary>
    public DbSet<Payment> Payments => Set<Payment>();

    /// <summary>Gets the set of organizer debt records.</summary>
    public DbSet<OrganizerDebt> OrganizerDebts => Set<OrganizerDebt>();

    /// <summary>Gets the set of padel sites.</summary>
    public DbSet<Site> Sites => Set<Site>();

    /// <summary>Gets the set of courts.</summary>
    public DbSet<Court> Courts => Set<Court>();

    /// <summary>Gets the set of site schedules.</summary>
    public DbSet<SiteSchedule> SiteSchedules => Set<SiteSchedule>();

    /// <summary>Gets the set of site closures.</summary>
    public DbSet<SiteClosure> SiteClosures => Set<SiteClosure>();

    /// <summary>
    /// Configures entity mappings by applying all <see cref="IEntityTypeConfiguration{TEntity}"/>
    /// implementations found in the infrastructure assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadTimeDbContext).Assembly);

        // Analytics schema
        modelBuilder.HasDefaultSchema("public");
    }

    /// <summary>
    /// Saves all pending changes to the database after dispatching accumulated domain events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker
            .Entries<Entity<Guid>>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(new DomainEventNotification(domainEvent), cancellationToken);
        }
    }
}