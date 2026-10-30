using System.Net.Http.Json;
using System.Text.Json;
using Saksbehandling.Api.Tests.TestInfrastructure;

namespace Saksbehandling.Api.Tests.Functional;

[Collection("Sequential")]
public class CommentsAndDocumentsTests
{
    private readonly CustomWebApplicationFactory _factory;

    public CommentsAndDocumentsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddComment_ThenListIncludesIt()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1");
        var caseId = await client.CreateOwnCaseAsync("Sak for kommentartest");

        var addResponse = await client.PostAsJsonAsync($"/api/v2/cases/{caseId}/comments", new
        {
            text = "Dette er en vanlig, ordinær kommentar.",
            isInternal = false,
        });
        addResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync($"/api/v2/cases/{caseId}/comments");
        listResponse.EnsureSuccessStatusCode();
        var comments = await listResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Contains(
            comments.EnumerateArray(),
            c => c.GetProperty("text").GetString() == "Dette er en vanlig, ordinær kommentar.");
    }
}
