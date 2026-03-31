// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Common;

/// <summary>
/// MediatR notification wrapper for domain events, enabling cross-cutting event handling in the application layer.
/// </summary>
/// <param name="DomainEvent">The domain event to broadcast.</param>
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;