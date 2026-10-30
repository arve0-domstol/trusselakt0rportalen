using System.Net.Http.Json;

namespace Saksbehandling.Api.Tests.TestInfrastructure;

public static class TestAuth
{
    public const string SharedPassword = "Trening2026!";

    /// <summary>Returns a cookie-aware client logged in as the given seeded user.</summary>
    public static async Task<HttpClient> LoginAsAsync(
        this CustomWebApplicationFactory factory,
        string username,
        string password = SharedPassword)
    {
        var client = factory.CreateClient(new() { HandleCookies = true });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password,
            rememberMe = false,
        });

        response.EnsureSuccessStatusCode();
        return client;
    }
}
