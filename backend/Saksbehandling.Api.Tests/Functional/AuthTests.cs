using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Saksbehandling.Api.Tests.TestInfrastructure;

namespace Saksbehandling.Api.Tests.Functional;

[Collection("Sequential")]
public class AuthTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithCorrectPassword_Succeeds()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "saksbehandler1",
            password = TestAuth.SharedPassword,
            rememberMe = false,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("saksbehandler1", profile.GetProperty("username").GetString());
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "saksbehandler1",
            password = "not-the-right-password",
            rememberMe = false,
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutSession_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_InvalidatesTheClientSideCookie()
    {
        var client = await _factory.LoginAsAsync("saksbehandler3");

        var meBeforeLogout = await client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.OK, meBeforeLogout.StatusCode);

        var logoutResponse = await client.PostAsync("/api/auth/logout", content: null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var meAfterLogout = await client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_RequestConfirmAndLoginWithNewPassword_RoundTrips()
    {
        var client = _factory.CreateClient();
        const string newPassword = "NyttPassordForTest1";

        var requestResponse = await client.PostAsJsonAsync("/api/auth/reset-password/request", new
        {
            usernameOrEmail = "saksbehandler9",
        });
        requestResponse.EnsureSuccessStatusCode();
        var requestBody = await requestResponse.Content.ReadFromJsonAsync<JsonElement>();
        var resetToken = requestBody.GetProperty("resetToken").GetString();
        Assert.False(string.IsNullOrEmpty(resetToken));

        var confirmResponse = await client.PostAsJsonAsync("/api/auth/reset-password/confirm", new
        {
            token = resetToken,
            newPassword,
        });
        confirmResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "saksbehandler9",
            password = newPassword,
            rememberMe = false,
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }
}
