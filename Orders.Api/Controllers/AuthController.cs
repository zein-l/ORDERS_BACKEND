using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;

// IMPORTANT: If you have `using Orders.Api.ProblemDetails;` anywhere, remove it.
// Alias the MVC type so we never collide with your ProblemDetails namespace.
using PD = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Register a new user and get a JWT.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        try
        {
            var res = await _auth.RegisterAsync(req, ct);
            return Ok(res);
        }
        catch (InvalidOperationException ex)
        {
            // Email already exists scenario
            var pd = new PD
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already registered",
                Detail = ex.Message
            };
            return Conflict(pd);
        }
    }

    /// <summary>Login with email/password and get a JWT.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        try
        {
            var res = await _auth.LoginAsync(req, ct);
            return Ok(res);
        }
        catch (InvalidOperationException ex)
        {
            // Map service messages to precise HTTP errors
            var msg = ex.Message ?? string.Empty;

            if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                var pd = new PD
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "User not found",
                    Detail = "No account exists for the provided email."
                };
                return NotFound(pd);
            }

            if (msg.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                var pd = new PD
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid credentials",
                    Detail = "The password is incorrect."
                };
                return Unauthorized(pd);
            }

            // Fallback (generic)
            var fallback = new PD
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Login failed",
                Detail = "Invalid email or password."
            };
            return Unauthorized(fallback);
        }
    }
}
