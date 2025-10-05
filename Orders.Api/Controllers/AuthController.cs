using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // endpoints in this controller are public
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest req,
        CancellationToken ct)
    {
        try
        {
            var res = await _auth.RegisterAsync(req, ct);
            return Ok(res);
        }
        catch (InvalidOperationException ex)
        {
            // e.g., email already exists
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest req,
        CancellationToken ct)
    {
        try
        {
            var res = await _auth.LoginAsync(req, ct);
            return Ok(res);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
    }
}
