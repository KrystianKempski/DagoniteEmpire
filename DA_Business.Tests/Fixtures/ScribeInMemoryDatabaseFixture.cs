using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Tests.Fixtures;

/// <summary>
/// In-memory database fixture for Scribe tests. The Scribe schema uses
/// pgvector's `vector(768)` column type for embeddings which SQLite cannot map,
/// so we fall back to the EF Core in-memory provider which ignores DB types.
/// In-memory provider is fine here since we never exercise vector search itself
/// (those tests would need a real PostgreSQL with pgvector).
/// </summary>
public class ScribeInMemoryDatabaseFixture : IDisposable
{
    public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; }

    public ScribeInMemoryDatabaseFixture()
    {
        var dbName = "scribe-tests-" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        DbContextFactory = new InMemoryDbContextFactory(options);
    }

    public ApplicationDbContext CreateContext() => DbContextFactory.CreateDbContext();

    public void Dispose() { }

    private sealed class InMemoryDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        public InMemoryDbContextFactory(DbContextOptions<ApplicationDbContext> options) => _options = options;
        public ApplicationDbContext CreateDbContext() => new(_options);
    }
}
