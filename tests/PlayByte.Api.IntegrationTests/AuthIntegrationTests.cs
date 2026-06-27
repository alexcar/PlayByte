using System.Net;
using System.Net.Http.Json;
using PlayByte.Application.Authentication.Login;
using Shouldly;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

public sealed class AuthIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        var client = Factory.CreateClient();
        var (_, email, password) = await RegisterUserAsync(client);

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ComSenhaErrada_DeveRetornar401()
    {
        var client = Factory.CreateClient();
        var (_, email, _) = await RegisterUserAsync(client);

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Errada@999" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenEmitido_DeveAutorizarEndpointProtegido()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Usuario novo nao tem assinatura: o esperado e' 404 (autorizado e tratado), nao 401.
        var response = await client.GetAsync("/api/subscriptions/me");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForgotPassword_DeveSempreRetornar200()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email = "inexistente@playbyte.com" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
