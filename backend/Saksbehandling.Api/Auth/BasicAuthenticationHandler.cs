using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Saksbehandling.Api.Auth;

// A static memory store to hold the secrets fetched from the internal service
public static class SecretStore
{
    public static string Username { get; set; } = string.Empty;
    public static string Password { get; set; } = string.Empty;
}

// Helper classes to deserialize the internal service json format
public class AdminConfig
{
    public CredentialsData AdminCredentials { get; set; } = new();

    public class CredentialsData
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            
            if (authHeader.Scheme != "Basic" || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return AuthenticateResult.Fail("Invalid Authorization Scheme");
            }

            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
            var username = credentials[0];
            var password = credentials[1];

            if (username == SecretStore.Username && password == SecretStore.Password && !string.IsNullOrEmpty(username))
            {
                var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Administrator")
                };
                
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.Fail("Invalid Username or Password");
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header Format");
        }
    }
    
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // When this controller lists more than one authentication scheme,
        // the framework challenges all of them in order on a failed
        // request - only the first one to actually write a response should
        // shape it, so later handlers back off once that's happened.
        if (Response.HasStarted)
        {
            return;
        }

        Response.StatusCode = 401;

        Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Admin Panel\"");

        await Response.WriteAsync("401 Unauthorized: Administrative credentials required.");
    }
}