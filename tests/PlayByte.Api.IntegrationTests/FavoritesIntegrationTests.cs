using System.Net;
using System.Net.Http.Json;
using PlayByte.Application.Favorites.Bands;
using PlayByte.Application.Favorites.Tracks;
using Shouldly;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

public sealed class FavoritesIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task FavoritarBanda_DepoisListar_DeveConterABanda()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bandId = await CreateBandAsync(client, $"FavBand {Guid.NewGuid():N}");

        var favorite = await client.PostAsync($"/api/favorites/bands/{bandId}", content: null);
        favorite.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var list = await client.GetAsync("/api/favorites/bands");
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        var bands = await list.Content.ReadFromJsonAsync<List<FavoriteBandItem>>();
        bands!.ShouldContain(b => b.BandId == bandId);
    }

    [Fact]
    public async Task FavoritarFaixa_DepoisListar_DeveConterAFaixa()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bandId = await CreateBandAsync(client, $"FavTrackBand {Guid.NewGuid():N}");
        var albumId = await CreateAlbumAsync(client, bandId, "Album");
        var trackId = await CreateTrackAsync(client, bandId, albumId, "Faixa");

        var favorite = await client.PostAsync($"/api/favorites/tracks/{trackId}", content: null);
        favorite.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var list = await client.GetAsync("/api/favorites/tracks");
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tracks = await list.Content.ReadFromJsonAsync<List<FavoriteTrackItem>>();
        tracks!.ShouldContain(t => t.TrackId == trackId);
    }

    [Fact]
    public async Task DesfavoritarBanda_DeveRetornar204()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bandId = await CreateBandAsync(client, $"UnfavBand {Guid.NewGuid():N}");
        await client.PostAsync($"/api/favorites/bands/{bandId}", content: null);

        var response = await client.DeleteAsync($"/api/favorites/bands/{bandId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
