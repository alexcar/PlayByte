using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayByte.Api.Extensions;
using PlayByte.Application.Users.Commands.ChangePassword;
using PlayByte.Application.Users.Commands.RegisterUser;
using PlayByte.Application.Users.Commands.UpdateProfile;
using PlayByte.Application.Users.Queries.GetUserById;

namespace PlayByte.Api.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Registra um novo usuario.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : ApiResults.Problem(result.Error);
    }

    /// <summary>Obtem um usuario por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.Problem(result.Error);
    }

    /// <summary>Atualiza o nome e o e-mail do usuario autenticado.</summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateUserProfileCommand(User.GetUserId(), request.Name, request.Email);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.Problem(result.Error);
    }

    /// <summary>Altera a senha do usuario autenticado (exige a senha atual).</summary>
    [HttpPut("me/password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var command = new ChangePasswordCommand(User.GetUserId(), request.CurrentPassword, request.NewPassword);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? NoContent()
            : ApiResults.Problem(result.Error);
    }
}

/// <summary>Payload de registro de usuario.</summary>
public sealed record RegisterUserRequest(string Name, string Email, string Password);

/// <summary>Payload de atualizacao de perfil.</summary>
public sealed record UpdateProfileRequest(string Name, string Email);

/// <summary>Payload de troca de senha.</summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
