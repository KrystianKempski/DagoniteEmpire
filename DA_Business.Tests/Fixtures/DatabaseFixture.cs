using AutoMapper;
using DA_Business.Mapper;
using DA_DataAccess.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DA_Business.Tests.Fixtures;

/// <summary>
/// Provides a shared SQLite in-memory database for repository tests.
/// Uses IClassFixture to share the database across all tests in a test class.
/// </summary>
public class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; }
    public IMapper Mapper { get; }

    public DatabaseFixture()
    {
        // Create and open a connection - keeping it open preserves the in-memory database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the database factory
        DbContextFactory = new TestDbContextFactory(options);

        // Initialize the database schema
        using var context = DbContextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        // Configure AutoMapper v15+ - requires MapperConfigurationExpression + ILoggerFactory
        var configExpression = new MapperConfigurationExpression();
        configExpression.AddProfile<MappingProfile>();
        var config = new MapperConfiguration(configExpression, NullLoggerFactory.Instance);
        Mapper = config.CreateMapper();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    /// <summary>
    /// Creates a fresh database context for each test
    /// </summary>
    public ApplicationDbContext CreateContext()
    {
        return DbContextFactory.CreateDbContext();
    }

    /// <summary>
    /// Resets the database to a clean state between tests
    /// </summary>
    public void ResetDatabase()
    {
        using var context = DbContextFactory.CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}

/// <summary>
/// Simple implementation of IDbContextFactory for testing
/// </summary>
internal class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
    {
        _options = options;
    }

    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(_options);
    }

    public Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }
}
