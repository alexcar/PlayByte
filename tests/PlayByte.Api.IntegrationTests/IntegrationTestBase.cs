using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PlayByte.Application.Authentication.Login;
using Shouldly;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

/// <summary>Base das classes de teste de integracao: fluxos HTTP auxiliares reutilizaveis.</summary>
[Collection(IntegrationCollection.Name)]
public abstract class IntegrationTestBase(IntegrationTestFactory factory)
{
    protected IntegrationTestFactory Factory { get; } = factory;

    protected static (string Name, string Email, string Password) NewUser()
        => ("Usuario Teste", $"user-{Guid.NewGuid():N}@playbyte.com", "Senha@123");

    /// <summary>Registra um usuario e devolve o id criado.</summary>
    protected async Task<(Guid Id, string Email, string Password)> RegisterUserAsync(HttpClient client)
    {
        var (name, email, password) = NewUser();
        var response = await client.PostAsJsonAsync("/api/users", new { name, email, password });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return (id, email, password);
    }

    /// <summary>Cria um HttpClient ja autenticado (registra + faz login + injeta o Bearer).</summary>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = Factory.CreateClient();
        var (_, email, password) = await RegisterUserAsync(client);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    protected static async Task<Guid> CreateBandAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/bands", new { name, coverImageUrl = (string?)null });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreatedId>())!.Id;
    }

    protected static async Task<Guid> CreateAlbumAsync(HttpClient client, Guid bandId, string title)
    {
        var response = await client.PostAsJsonAsync($"/api/bands/{bandId}/albums", new { title, releaseYear = 1990 });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<CreatedId>())!.Id;
    }

    protected static async Task<Guid> CreateTrackAsync(HttpClient client, Guid bandId, Guid albumId, string title)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/bands/{bandId}/albums/{albumId}/tracks", new { title, durationSeconds = 200 });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<CreatedId>())!.Id;
    }
}

/// <summary>Forma generica do corpo { id } devolvido pelos endpoints de cadastro do catalogo.</summary>
internal sealed record CreatedId(Guid Id);
