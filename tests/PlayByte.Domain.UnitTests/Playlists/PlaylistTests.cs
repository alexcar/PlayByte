using PlayByte.Domain.Catalog;
using PlayByte.Domain.Playlists;
using PlayByte.Domain.Users;
using Shouldly;
using Xunit;

namespace PlayByte.Domain.UnitTests.Playlists;

public sealed class PlaylistTests
{
    [Fact]
    public void Create_ComNomeVazio_DeveFalhar()
    {
        var result = Playlist.Create(UserId.New(), "  ");
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PlaylistErrors.NameRequired);
    }

    [Fact]
    public void AddTrack_FaixaDuplicada_DeveFalharSemDuplicar()
    {
        var playlist = Playlist.Create(UserId.New(), "Favoritas").Value;
        var trackId = TrackId.New();

        playlist.AddTrack(trackId, DateTimeOffset.UtcNow).IsSuccess.ShouldBeTrue();
        var segunda = playlist.AddTrack(trackId, DateTimeOffset.UtcNow);

        segunda.IsFailure.ShouldBeTrue();
        segunda.Error.ShouldBe(PlaylistErrors.TrackAlreadyAdded);
        playlist.TrackCount.ShouldBe(1); // US-13 c3/c4
    }

    [Fact]
    public void AddTrack_FaixasDistintas_DeveIncrementarPosicao()
    {
        var playlist = Playlist.Create(UserId.New(), "Rock").Value;

        playlist.AddTrack(TrackId.New(), DateTimeOffset.UtcNow);
        playlist.AddTrack(TrackId.New(), DateTimeOffset.UtcNow);

        playlist.TrackCount.ShouldBe(2);
        playlist.Items[^1].Position.ShouldBe(2);
    }
}
