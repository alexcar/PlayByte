using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Catalog.Queries.GetBandDetails;

public sealed record GetBandDetailsQuery(Guid BandId) : IQuery<BandDetailsResponse>;

public sealed record BandDetailsResponse(Guid Id, string Name, string? CoverImageUrl, IReadOnlyList<AlbumDto> Albums);
public sealed record AlbumDto(Guid Id, string Title, int ReleaseYear, IReadOnlyList<TrackDto> Tracks);
public sealed record TrackDto(Guid Id, string Title, int DurationSeconds);
