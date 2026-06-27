using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Domain.Common;
using Shouldly;

namespace PlayByte.Api.UnitTests;

/// <summary>Utilitarios compartilhados pelos testes de Controller.</summary>
internal static class ControllerTestExtensions
{
    /// <summary>
    /// Anexa um usuario autenticado ao Controller (claim "sub" = userId), para que
    /// <c>User.GetUserId()</c> funcione nos endpoints protegidos.
    /// </summary>
    public static T WithUser<T>(this T controller, Guid userId) where T : ControllerBase
    {
        var identity = new ClaimsIdentity([new Claim("sub", userId.ToString())], "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    /// <summary>
    /// Verifica que o resultado e' um ProblemDetails (via <c>ApiResults.Problem</c>) com o
    /// status HTTP esperado e devolve o ProblemDetails para asserts adicionais.
    /// </summary>
    public static ProblemDetails ShouldBeProblem(this IActionResult result, int expectedStatusCode)
    {
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(expectedStatusCode);
        return objectResult.Value.ShouldBeOfType<ProblemDetails>();
    }
}
