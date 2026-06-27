using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Application.Users.Queries.GetUserById;
using Shouldly;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

public sealed class UsersIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Registrar_DepoisConsultar_DeveRetornarOUsuarioPersistido()
    {
        var client = Factory.CreateClient();
        var (id, email, _) = await RegisterUserAsync(client);

        var response = await client.GetAsync($"/api/users/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user!.Id.ShouldBe(id);
        user.Email.ShouldBe(email);
    }

    [Fact]
    public async Task Registrar_ComEmailInvalido_DeveRetornar400ProblemDetails()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users",
            new { name = "Teste", email = "email-invalido", password = "Senha@123" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.ShouldBe(400);
    }

    [Fact]
    public async Task Registrar_ComEmailDuplicado_DeveRetornar409()
    {
        var client = Factory.CreateClient();
        var (name, email, password) = NewUser();
        (await client.PostAsJsonAsync("/api/users", new { name, email, password }))
            .StatusCode.ShouldBe(HttpStatusCode.Created);

        var duplicate = await client.PostAsJsonAsync("/api/users", new { name, email, password });

        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Consultar_UsuarioInexistente_DeveRetornar404()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
