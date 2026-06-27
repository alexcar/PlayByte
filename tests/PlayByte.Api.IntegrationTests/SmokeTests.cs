using System.Net;
using Shouldly;
using Xunit;

namespace PlayByte.Api.IntegrationTests;

public sealed class SmokeTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task HealthCheck_DeveResponder200()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_DeveRetornar401()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/subscriptions/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
