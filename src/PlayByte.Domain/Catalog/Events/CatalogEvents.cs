using PlayByte.Domain.Abstractions;

namespace PlayByte.Domain.Catalog.Events;

public sealed record BandCreated(BandId BandId, string Name) : IDomainEvent;
