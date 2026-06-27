using PlayByte.Domain.Catalog;
using Shouldly;
using Xunit;

namespace PlayByte.Domain.UnitTests.Catalog;

public sealed class BandTests
{
    [Fact]
    public void Create_SemNome_DeveFalhar()
    {
        var result = Band.Create(" ", null);
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CatalogErrors.BandNameRequired);
    }

    [Fact]
    public void AddTrack_EmAlbumInexistente_DeveFalhar()
    {
        var band = Band.Create("Metallica", null).Value;

        var result = band.AddTrack(AlbumId.New(), "One", 446);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CatalogErrors.AlbumNotFound);
    }

    [Fact]
    public void AddAlbumEThenTrack_DeveAninharFaixaNoAlbum()
    {
        var band = Band.Create("Metallica", null).Value;
        var album = band.AddAlbum("...And Justice for All", 1988).Value;

        var track = band.AddTrack(album.Id, "Blackened", 403);

        track.IsSuccess.ShouldBeTrue();
        band.Albums.ShouldHaveSingleItem();
        band.Albums[0].Tracks.ShouldHaveSingleItem();
        band.Albums[0].Tracks[0].DurationSeconds.ShouldBe(403);
    }
}
