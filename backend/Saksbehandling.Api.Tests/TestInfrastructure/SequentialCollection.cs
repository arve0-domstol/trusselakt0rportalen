namespace Saksbehandling.Api.Tests.TestInfrastructure;

/// <summary>
/// All test classes share this collection so they run sequentially against
/// one test database/factory instance, instead of xUnit's default of
/// parallelizing across test classes - safer for an integration suite where
/// every test shares the same underlying Postgres database.
/// </summary>
[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
