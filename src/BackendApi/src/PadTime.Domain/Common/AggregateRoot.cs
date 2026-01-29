namespace PadTime.Domain.Common;

/// <summary>
/// Base class for aggregate roots.
/// Aggregate roots are the only entities that can be directly accessed from repositories.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    public uint Version { get; protected set; }
}
