using PlayByte.Application.Abstractions.Messaging;

namespace PlayByte.Application.Catalog.Playback;

public sealed record PlayTrackQuery(Guid UserId, Guid TrackId) : IQuery<PlaybackResponse>;

public sealed record PlaybackResponse(Guid TrackId, string Title, string BandName, string StreamUrl);
