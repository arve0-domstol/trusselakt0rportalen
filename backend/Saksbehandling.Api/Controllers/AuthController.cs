using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Services;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var (user, error, statusCode) = await _authService.AuthenticateAsync(request.Username, request.Password);
        if (user is null)
        {
            return StatusCode(statusCode, new { message = error });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("departmentId", user.DepartmentId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = request.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddDays(7),
        });

        return Ok(UserProfileDto.From(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpPost("reset-password/request")]
    public async Task<IActionResult> RequestPasswordReset(ForgotPasswordRequest request)
    {
        var resetToken = await _authService.CreatePasswordResetTokenAsync(request.UsernameOrEmail);
        return Ok(new
        {
            message = "Hvis brukeren finnes, er det sendt instruksjoner for å tilbakestille passordet.",
            resetToken,
        });
    }

    [HttpPost("reset-password/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset(ResetPasswordConfirmRequest request)
    {
        var (success, error) = await _authService.ConfirmPasswordResetAsync(request.Token, request.NewPassword);
        if (!success)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Passordet er oppdatert." });
    }
}
