using System.Net;
using System.Net.Http.Json;
using PlayByte.Application.Playlists.Queries.GetPlaylist;
using PlayByte.Application.Playlists.Queries.GetUserPlaylists;
using Shouldly;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

public sealed class PlaylistsIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CriarListarEObter_PlaylistDoUsuario()
    {
        var client = await CreateAuthenticatedClientAsync();

        var create = await client.PostAsJsonAsync("/api/playlists", new { name = "Treino" });
        create.StatusCode.ShouldBe(HttpStatusCode.Created);
        var playlistId = (await create.Content.ReadFromJsonAsync<CreatedId>())!.Id;

        var list = await client.GetAsync("/api/playlists");
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        var summaries = await list.Content.ReadFromJsonAsync<List<PlaylistSummary>>();
        summaries!.ShouldContain(p => p.Id == playlistId && p.Name == "Treino");

        var details = await client.GetAsync($"/api/playlists/{playlistId}");
        details.StatusCode.ShouldBe(HttpStatusCode.OK);
        var response = await details.Content.ReadFromJsonAsync<PlaylistDetailsResponse>();
        response!.Id.ShouldBe(playlistId);
        response.Name.ShouldBe("Treino");
    }

    [Fact]
    public async Task AdicionarFaixa_DepoisDuplicar_DeveRetornar409()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bandId = await CreateBandAsync(client, $"PL {Guid.NewGuid():N}");
        var albumId = await CreateAlbumAsync(client, bandId, "Album");
        var trackId = await CreateTrackAsync(client, bandId, albumId, "Faixa");

        var create = await client.PostAsJsonAsync("/api/playlists", new { name = "Minha" });
        var playlistId = (await create.Content.ReadFromJsonAsync<CreatedId>())!.Id;

        var first = await client.PostAsJsonAsync($"/api/playlists/{playlistId}/tracks", new { trackId });
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        var duplicate = await client.PostAsJsonAsync($"/api/playlists/{playlistId}/tracks", new { trackId });
        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}
