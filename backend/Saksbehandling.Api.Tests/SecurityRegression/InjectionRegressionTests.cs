using System.Net.Http.Json;
using System.Text.Json;
using Saksbehandling.Api.Tests.TestInfrastructure;

namespace Saksbehandling.Api.Tests.SecurityRegression;

// See SecurityRegression/README.md for what these tests are and how to use
// them once you've fixed the underlying issue.
[Collection("Sequential")]
[Trait("Category", "SecurityRegressionTrainingFixture")]
public class InjectionRegressionTests
{
    private readonly CustomWebApplicationFactory _factory;

    public InjectionRegressionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// V12 - the free-text case search is built with raw string
    /// concatenation. A boolean-breakout payload neutralizes the
    /// department filter entirely, returning every case in the system
    /// instead of only the caller's own department.
    /// </summary>
    [Fact]
    public async Task V12_CaseSearch_SqlInjectionBypassesDepartmentScope()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1"); // Kundeservice, ~14-15 cases

        var scopedResponse = await client.GetAsync("/api/v2/cases?pageSize=1");
        scopedResponse.EnsureSuccessStatusCode();
        var scoped = await scopedResponse.Content.ReadFromJsonAsync<JsonElement>();
        var scopedCount = scoped.GetProperty("totalCount").GetInt32();

        var payload = Uri.EscapeDataString("x') OR (1=1) --");
        var injectedResponse = await client.GetAsync($"/api/v2/cases?search={payload}&pageSize=1");
        injectedResponse.EnsureSuccessStatusCode();
        var injected = await injectedResponse.Content.ReadFromJsonAsync<JsonElement>();
        var injectedCount = injected.GetProperty("totalCount").GetInt32();

        Assert.True(
            injectedCount > scopedCount,
            $"Expected the injection to return more rows than the department-scoped count ({scopedCount}), but got {injectedCount}.");
    }

    /// <summary>
    /// V15 - case comments are stored and returned exactly as submitted,
    /// with no output encoding at any layer. This is the data-layer half
    /// of the stored-XSS chain (the frontend renders this text via
    /// dangerouslySetInnerHTML - see docs/VULNERABILITY-MATRIX.md V15).
    /// </summary>
    [Fact]
    public async Task V15_Comment_StoredAndReturnedWithoutSanitization()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1");
        var caseId = await client.CreateOwnCaseAsync("Sak for XSS-regresjonstest");
        const string payload = "<img src=x onerror=alert(document.cookie)>";

        var addResponse = await client.PostAsJsonAsync($"/api/v2/cases/{caseId}/comments", new
        {
            text = payload,
            isInternal = false,
        });
        addResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync($"/api/v2/cases/{caseId}/comments");
        var comments = await listResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Contains(
            comments.EnumerateArray(),
            c => c.GetProperty("text").GetString() == payload);
    }
}
