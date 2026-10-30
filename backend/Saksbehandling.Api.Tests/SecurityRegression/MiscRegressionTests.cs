using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Saksbehandling.Api.Tests.TestInfrastructure;

namespace Saksbehandling.Api.Tests.SecurityRegression;

// See SecurityRegression/README.md for what these tests are and how to use
// them once you've fixed the underlying issue.
[Collection("Sequential")]
[Trait("Category", "SecurityRegressionTrainingFixture")]
public class MiscRegressionTests
{
    private readonly CustomWebApplicationFactory _factory;

    public MiscRegressionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// V24 + V30 - the "Importer dokument fra URL" feature fetches any URL
    /// with no allowlist, and attaches a hard-coded internal API key to
    /// every outbound request regardless of destination. This test stands
    /// in for fetching the real internal-mock-service (which only resolves
    /// inside the Docker Compose network, not from wherever `dotnet test`
    /// happens to run) with a throwaway local HTTP listener representing
    /// "a resource that shouldn't be reachable from outside" - it exercises
    /// the exact same application code path. The manual verification
    /// against the real internal-mock-service is recorded in
    /// docs/VULNERABILITY-MATRIX.md (V24, V30) and training/INSTRUCTOR_GUIDE.md.
    /// </summary>
    [Fact]
    public async Task V24AndV30_ImportFromUrl_FetchesArbitraryUrlAndLeaksInternalApiKey()
    {
        const string marker = "simulert-internt-innhold-12345";
        string? capturedApiKeyHeader = null;

        using var listener = new HttpListener();
        var port = GetFreeTcpPort();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        var listenTask = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            capturedApiKeyHeader = context.Request.Headers["X-Internal-Api-Key"];

            var buffer = Encoding.UTF8.GetBytes(marker);
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.OutputStream.Close();
        });

        var client = await _factory.LoginAsAsync("saksbehandler1");
        var caseId = await client.CreateOwnCaseAsync("Sak for SSRF-regresjonstest");

        var importResponse = await client.PostAsJsonAsync($"/api/v2/cases/{caseId}/documents/import-url", new
        {
            url = $"http://127.0.0.1:{port}/",
        });
        importResponse.EnsureSuccessStatusCode();
        await listenTask;
        listener.Stop();

        var imported = await importResponse.Content.ReadFromJsonAsync<JsonElement>();
        var documentId = imported.GetProperty("id").GetInt32();

        var downloadResponse = await client.GetAsync($"/api/documents/{documentId}");
        var downloadedContent = await downloadResponse.Content.ReadAsStringAsync();

        Assert.Equal(marker, downloadedContent);
        Assert.Equal("TRAINING-ONLY-SECRET-OUTBOUND-API-KEY-0002", capturedApiKeyHeader);
    }

    /// <summary>
    /// V40 - the audit-log cleanup endpoint has no role check and no
    /// lower bound on how much history it can remove in one call.
    /// </summary>
    [Fact]
    public async Task V40_AuditLogCleanup_AllowsNonAdminToPurgeEverything()
    {
        var client = await _factory.LoginAsAsync("saksbehandler12"); // role: SeniorSaksbehandler, not Administrator
        var caseId = await client.CreateOwnCaseAsync("Sak for revisjonslogg-test");

        var historyBefore = await client.GetAsync($"/api/v2/cases/{caseId}/history");
        var beforeEvents = await historyBefore.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEmpty(beforeEvents.EnumerateArray()); // at least the "Opprettet" event

        var purgeResponse = await client.DeleteAsync("/api/admin/audit-log?olderThanDays=0");
        Assert.Equal(HttpStatusCode.OK, purgeResponse.StatusCode);

        var historyAfter = await client.GetAsync($"/api/v2/cases/{caseId}/history");
        var afterEvents = await historyAfter.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Empty(afterEvents.EnumerateArray());
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
