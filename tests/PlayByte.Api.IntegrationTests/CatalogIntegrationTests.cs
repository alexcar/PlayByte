using System.Net;
using System.Net.Http.Json;
using PlayByte.Application.Abstractions.Pagination;
using PlayByte.Application.Catalog.Queries.GetBandDetails;
using PlayByte.Application.Catalog.Queries.ListBands;
using PlayByte.Application.Search;
using Shouldly;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

public sealed class CatalogIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CriarBanda_DepoisDetalhar_DeveRetornarBandaComAlbunsEFaixas()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bandName = $"Metallica {Guid.NewGuid():N}";

        var bandId = await CreateBandAsync(client, bandName);
        var albumId = await CreateAlbumAsync(client, bandId, "Master of Puppets");
        await CreateTrackAsync(client, bandId, albumId, "Battery");

        var response = await client.GetAsync($"/api/bands/{bandId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content.ReadFromJsonAsync<BandDetailsResponse>();
        details!.Name.ShouldBe(bandName);
        var album = details.Albums.ShouldHaveSingleItem();
        album.Title.ShouldBe("Master of Puppets");
        album.Tracks.ShouldContain(t => t.Title == "Battery");
    }

    [Fact]
    public async Task ListarBandas_DeveIncluirBandaCriada()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bandName = $"Iron Maiden {Guid.NewGuid():N}";
        var bandId = await CreateBandAsync(client, bandName);

        var response = await client.GetAsync("/api/bands?page=1&pageSize=100");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<BandListItem>>();
        page!.Items.ShouldContain(b => b.Id == bandId);
    }

    [Fact]
    public async Task Buscar_PorNomeDaBanda_DeveEncontrar()
    {
        var client = await CreateAuthenticatedClientAsync();
        var unique = Guid.NewGuid().ToString("N");
        var bandId = await CreateBandAsync(client, $"BuscaBand {unique}");

        var response = await client.GetAsync($"/api/search?q={unique}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SearchResponse>();
        result!.Bands.ShouldContain(b => b.Id == bandId);
    }

    [Fact]
    public async Task ReproduzirFaixa_ComUsuarioGratuito_DeveRetornar403()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bandId = await CreateBandAsync(client, $"PlayBand {Guid.NewGuid():N}");
        var albumId = await CreateAlbumAsync(client, bandId, "Album");
        var trackId = await CreateTrackAsync(client, bandId, albumId, "Faixa");

        var response = await client.PostAsync($"/api/tracks/{trackId}/play", content: null);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
