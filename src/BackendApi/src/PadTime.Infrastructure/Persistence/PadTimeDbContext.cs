using MediatR;
using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Infrastructure.Persistence;

public sealed class PadTimeDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    public PadTimeDbContext(DbContextOptions<PadTimeDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    // Booking
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<SiteYearSchedule> SiteYearSchedules => Set<SiteYearSchedule>();
    public DbSet<Closure> Closures => Set<Closure>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Participant> Participants => Set<Participant>();

    // Members
    public DbSet<Member> Members => Set<Member>();

    // Billing
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrganizerDebt> OrganizerDebts => Set<OrganizerDebt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadTimeDbContext).Assembly);

        // Analytics schema
        modelBuilder.HasDefaultSchema("public");
    }

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
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
