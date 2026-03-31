using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Common;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
