using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Saksbehandling.Api.Tests.TestInfrastructure;

namespace Saksbehandling.Api.Tests.Functional;

[Collection("Sequential")]
public class CasesTests
{
    private readonly CustomWebApplicationFactory _factory;

    public CasesTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task List_DefaultsToCallersOwnDepartment()
    {
        // saksbehandler1 is in Kundeservice (department 1).
        var client = await _factory.LoginAsAsync("saksbehandler1");

        var response = await client.GetAsync("/api/v2/cases?pageSize=50");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = result.GetProperty("items").EnumerateArray().ToList();
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal("Kundeservice", item.GetProperty("departmentName").GetString()));
    }

    [Fact]
    public async Task CreateThenGetById_RoundTrips()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1");

        var createResponse = await client.PostAsJsonAsync("/api/v2/cases", new
        {
            title = "Funksjonell test - opprettelse",
            description = "Opprettet av et automatisert funksjonelt API-test.",
            customerId = 1,
            priority = "Normal",
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();

        var getResponse = await client.GetAsync($"/api/v2/cases/{id}");
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Funksjonell test - opprettelse", fetched.GetProperty("title").GetString());
        Assert.Equal("Ny", fetched.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Update_ChangesTitleAndStatus()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1");

        var createResponse = await client.PostAsJsonAsync("/api/v2/cases", new
        {
            title = "Før oppdatering",
            description = "...",
            customerId = 1,
            priority = "Lav",
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();
        var departmentId = created.GetProperty("departmentId").GetInt32();

        var updateResponse = await client.PutAsJsonAsync($"/api/v2/cases/{id}", new
        {
            title = "Etter oppdatering",
            description = "Oppdatert beskrivelse.",
            status = "UnderBehandling",
            priority = "Hoy",
            assignedToUserId = (int?)null,
            departmentId,
        });
        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Etter oppdatering", updated.GetProperty("title").GetString());
        Assert.Equal("UnderBehandling", updated.GetProperty("status").GetString());
        Assert.Equal("Hoy", updated.GetProperty("priority").GetString());
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1");

        var response = await client.GetAsync("/api/v2/cases/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
