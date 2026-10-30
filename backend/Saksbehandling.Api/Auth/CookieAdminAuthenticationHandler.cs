using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Saksbehandling.Api.Auth;

public class CookieAdminAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public CookieAdminAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var cookieResult = await Context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!cookieResult.Succeeded || cookieResult.Principal is null)
        {
            return AuthenticateResult.Fail("No session");
        }

        if (!cookieResult.Principal.IsInRole("Administrator"))
        {
            return AuthenticateResult.Fail("Session does not have the required role");
        }

        return AuthenticateResult.Success(new AuthenticationTicket(cookieResult.Principal, Scheme.Name));
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
        return Task.CompletedTask;
    }
}
