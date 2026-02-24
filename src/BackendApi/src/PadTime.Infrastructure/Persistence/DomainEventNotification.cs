using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Infrastructure.Persistence;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
