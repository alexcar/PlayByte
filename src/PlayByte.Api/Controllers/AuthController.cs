using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Authentication.Login;
using PlayByte.Application.Authentication.PasswordReset;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[AllowAnonymous]
public sealed class AuthController(ISender sender) : ControllerBase
{
    /// <summary>Autentica e emite um token JWT (US-02).</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new LoginQuery(request.Email, request.Password), ct);
        return result.IsSuccess ? Ok(result.Value) : ApiResults.Problem(result.Error);
    }

    /// <summary>Solicita o link de recuperacao de senha (US-03). Resposta nao enumera contas.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await sender.Send(new RequestPasswordResetCommand(request.Email), ct);
        return Ok(new { message = "Se o e-mail estiver cadastrado, enviaremos um link de recuperacao." });
    }

    /// <summary>Redefine a senha a partir de um token valido (US-03).</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword), ct);
        return result.IsSuccess ? Ok() : ApiResults.Problem(result.Error);
    }
}

public sealed record LoginRequest(string Email, string Password);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);
