using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Saksbehandling.Api.Tests.TestInfrastructure;

/// <summary>
/// Runs the real application (migrations, seeding, everything in
/// Program.cs included) against a dedicated test database and a temp
/// storage directory, so the test suite never touches the database/volume
/// used for manual local development.
///
/// Requires a reachable Postgres server with the same credentials as local
/// dev (see appsettings.Development.json / .env.example) - typically
/// `docker compose up -d db` run once beforehand. The test database is
/// dropped and recreated from scratch at the start of every test run, so
/// repeated runs (e.g. while fixing a vulnerability and re-running tests)
/// are always starting from the same seeded state.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=saksbehandling_test;Username=saksbehandling;Password=TRAINING-ONLY-DB-PASSWORD-0001";

    private const string MaintenanceConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=saksbehandling;Password=TRAINING-ONLY-DB-PASSWORD-0001";

    public static readonly string TestStorageRoot =
        Path.Combine(Path.GetTempPath(), "saksbehandling-test-storage");

    public CustomWebApplicationFactory()
    {
        ResetTestDatabase();
        ResetTestStorage();
    }

    private static void ResetTestDatabase()
    {
        using var connection = new NpgsqlConnection(MaintenanceConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DROP DATABASE IF EXISTS saksbehandling_test WITH (FORCE);";
        command.ExecuteNonQuery();
    }

    private static void ResetTestStorage()
    {
        if (Directory.Exists(TestStorageRoot))
        {
            Directory.Delete(TestStorageRoot, recursive: true);
        }

        Directory.CreateDirectory(TestStorageRoot);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = TestConnectionString,
                ["Storage:RootPath"] = TestStorageRoot,
            });
        });
    }
}
