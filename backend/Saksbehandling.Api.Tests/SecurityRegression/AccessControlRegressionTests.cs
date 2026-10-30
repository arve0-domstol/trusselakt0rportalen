using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Saksbehandling.Api.Tests.TestInfrastructure;

namespace Saksbehandling.Api.Tests.SecurityRegression;

// See SecurityRegression/README.md for what these tests are and how to use
// them once you've fixed the underlying issue.
[Collection("Sequential")]
[Trait("Category", "SecurityRegressionTrainingFixture")]
public class AccessControlRegressionTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AccessControlRegressionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// V01 - case detail has no ownership/department check at all. A
    /// Kundeservice employee can fetch full detail on a case belonging to a
    /// different department just by knowing its ID.
    /// </summary>
    [Fact]
    public async Task V01_CaseDetail_AllowsCrossDepartmentAccess()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1"); // Kundeservice

        // Seeded deterministically - HR cases are the ones seeded into the
        // 10000+ id range, so this is guaranteed to be an HR case.
        var response = await client.GetAsync("/api/v2/cases/10000");
        response.EnsureSuccessStatusCode();

        var caseDetail = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEqual("Kundeservice", caseDetail.GetProperty("departmentName").GetString());
    }

    /// <summary>
    /// V06 - the case list defaults to the caller's own department, but an
    /// explicit departmentId query parameter is trusted without checking it
    /// matches the caller's own department.
    /// </summary>
    [Fact]
    public async Task V06_CaseList_DepartmentIdParameterBypassesDefaultScoping()
    {
        var client = await _factory.LoginAsAsync("saksbehandler1"); // Kundeservice, department 1

        var response = await client.GetAsync("/api/v2/cases?departmentId=3&pageSize=50"); // Juridisk
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = result.GetProperty("items").EnumerateArray().ToList();

        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal("Juridisk", item.GetProperty("departmentName").GetString()));
    }

    /// <summary>
    /// V02 - document download has no [Authorize] at all. A completely
    /// anonymous client can download any document by ID.
    /// </summary>
    [Fact]
    public async Task V02_DocumentDownload_RequiresNoAuthentication()
    {
        var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/documents/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// V03 - the admin users endpoint accepts the same shared internal API
    /// key used for service-to-service calls as a stand-in admin
    /// credential. Anyone who obtains that key - including via the
    /// SSRF-capable "import document from URL" feature, which attaches it
    /// to every outbound request it makes - can call the admin API
    /// directly, with no login at all.
    /// </summary>
    [Fact]
    public async Task V03_AdminUsersEndpoint_AcceptsInternalApiKeyWithNoLogin()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", "TRAINING-ONLY-SECRET-OUTBOUND-API-KEY-0002");

        var response = await client.GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// V04 (user-field manifestation) - the "edit my own profile" endpoint
    /// correctly checks the target ID matches the caller (no IDOR), but
    /// binds every field from the request body, including role. A user can
    /// self-promote to Administrator through a form that only shows
    /// name/email/phone.
    /// </summary>
    [Fact]
    public async Task V04_SelfProfileEdit_AcceptsRoleFieldChange()
    {
        var client = await _factory.LoginAsAsync("saksbehandler11"); // HR, role: Saksbehandler
        const int ownUserId = 16; // saksbehandler11, per the deterministic seed order

        var updateResponse = await client.PutAsJsonAsync($"/api/v2/users/{ownUserId}", new
        {
            fullName = "Håkon Fjeld",
            email = "hakon.fjeld@example-training.local",
            phone = "40012316",
            role = "Administrator", // not exposed in the real profile form
            departmentId = 5,
            isActive = true,
        });
        updateResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/v2/users/{ownUserId}");
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Administrator", fetched.GetProperty("role").GetString());
    }
}
