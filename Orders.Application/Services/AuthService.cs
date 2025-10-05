using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;   // IAuditService
using Orders.Domain.Entities;

namespace Orders.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _cfg;
    private readonly IAuditService _audit;
    private readonly PasswordHasher<User> _hasher;

    public AuthService(IUserRepository users, IConfiguration cfg, IAuditService audit)
    {
        _users = users;
        _cfg = cfg;
        _audit = audit;
        _hasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var existing = await _users.GetByEmailAsync(req.Email.ToLowerInvariant(), ct);
        if (existing is not null) throw new InvalidOperationException("Email already in use.");

        var user = new User(req.Email, "TEMP", req.FullName);
        var hash = _hasher.HashPassword(user, req.Password);
        user.SetPasswordHash(hash);

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new { user.Email });
        await _audit.LogAsync(new AuditEvent(user.Id, "UserRegistered", null, details), ct);

        return GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(req.Email.ToLowerInvariant(), ct);
        if (user is null) throw new InvalidOperationException("Invalid credentials.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed) throw new InvalidOperationException("Invalid credentials.");

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new { user.Email });
        await _audit.LogAsync(new AuditEvent(user.Id, "UserLoggedIn", null, details), ct);

        return GenerateToken(user);
    }

    private AuthResponse GenerateToken(User user)
    {
        var issuer = _cfg["Jwt:Issuer"]!;
        var audience = _cfg["Jwt:Audience"]!;
        var key = _cfg["Jwt:Key"]!;
        var expiresMinutes = int.TryParse(_cfg["Jwt:ExpiresMinutes"], out var m) ? m : 60;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new AuthResponse(jwt, token.ValidTo);
    }
}
