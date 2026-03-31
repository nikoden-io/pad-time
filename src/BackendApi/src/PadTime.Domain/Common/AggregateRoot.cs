// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Domain.Common;

/// <summary>
/// Base class for aggregate roots, the transactional consistency boundaries of the domain.
/// Aggregate roots are the only entities that can be directly referenced by repositories and
/// are responsible for enforcing invariants across their child entities.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    public uint Version { get; protected set; }
}