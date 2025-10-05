using Orders.Domain.Common;

namespace Orders.Domain.Entities;

public class User : BaseEntity
{
    // Basic auth fields (we'll do JWT later)
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? FullName { get; private set; }

    // Navigation
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    private User() { } // EF

    public User(string email, string passwordHash, string? fullName = null)
    {
        SetEmail(email);
        SetPasswordHash(passwordHash);
        FullName = fullName;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Invalid email.", nameof(email));
        Email = email.Trim().ToLowerInvariant();
        Touch();
    }

    public void SetPasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash is required.", nameof(hash));
        PasswordHash = hash;
        Touch();
    }
}
