using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Saksbehandling.Api.Auth;

public class InternalApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string HeaderName = "X-Internal-Api-Key";

    private readonly IConfiguration _configuration;

    public InternalApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration) : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var providedKey))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Missing {HeaderName} header"));
        }

        var expectedKey = _configuration["InternalApi:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) || providedKey != expectedKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid internal API key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "internal-service"),
            new Claim(ClaimTypes.Name, "internal-service"),
            new Claim(ClaimTypes.Role, "Administrator"),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // See the matching comment in BasicAuthenticationHandler - when
        // multiple schemes are challenged in sequence, back off once an
        // earlier one has already started the response.
        if (Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        Response.StatusCode = 401;
        return Response.WriteAsync("401 Unauthorized.");
    }
}
