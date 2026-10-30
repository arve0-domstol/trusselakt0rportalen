using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Auth;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, ILogger<AuthService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(User? User, string? Error, int StatusCode)> AuthenticateAsync(string username, string password)
    {
        var user = await _db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            return (null, "Fant ingen bruker med dette brukernavnet.", StatusCodes.Status401Unauthorized);
        }

        if (!user.IsActive)
        {
            return (null, "Kontoen er deaktivert.", StatusCodes.Status403Forbidden);
        }

        if (!PasswordHasher.Verify(password, user.PasswordHash))
        {
            return (null, "Feil passord.", StatusCodes.Status401Unauthorized);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (user, null, StatusCodes.Status200OK);
    }

    /// <summary>
    /// Starts a password reset for the given username/email, if a matching
    /// active account exists.
    /// </summary>
    public async Task<string?> CreatePasswordResetTokenAsync(string usernameOrEmail)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var code = new Random().Next(100_000, 999_999).ToString();

        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Generated password reset code for user {UserId}: {Code}", user.Id, code);

        return code;
    }

    public async Task<(bool Success, string? Error)> ConfirmPasswordResetAsync(string token, string newPassword)
    {
        if (newPassword.Length < 4)
        {
            return (false, "Passordet er for kort.");
        }

        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.Token == token)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (resetToken is null || resetToken.User is null || resetToken.UsedAt is not null || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return (false, "Koden er ugyldig eller utløpt.");
        }

        resetToken.User.PasswordHash = PasswordHasher.Hash(newPassword);
        resetToken.UsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, null);
    }
}
