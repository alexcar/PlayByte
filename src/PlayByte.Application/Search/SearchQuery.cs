using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Search;

public sealed record SearchQuery(string Term) : IQuery<SearchResponse>;

public sealed record SearchResponse(
    IReadOnlyList<SearchBandItem> Bands,
    IReadOnlyList<SearchTrackItem> Tracks);

public sealed record SearchBandItem(Guid Id, string Name, string? CoverImageUrl);
public sealed record SearchTrackItem(Guid Id, string Title, string BandName);
