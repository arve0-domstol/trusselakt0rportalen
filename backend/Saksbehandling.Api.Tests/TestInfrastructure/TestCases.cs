using System.Net.Http.Json;
using System.Text.Json;

namespace Saksbehandling.Api.Tests.TestInfrastructure;

public static class TestCases
{
    public static async Task<int> CreateOwnCaseAsync(this HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/v2/cases", new
        {
            title,
            description = "Opprettet av et automatisert API-test.",
            customerId = 1,
            priority = "Normal",
        });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        return created.GetProperty("id").GetInt32();
    }
}
