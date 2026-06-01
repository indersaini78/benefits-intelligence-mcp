using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace BenefitsIntelligence.IntegrationTests.Fixtures;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString
    {
        get
        {
            var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                InitialCatalog = "BenefitsIntelligenceDb",
                TrustServerCertificate = true
            };
            return builder.ConnectionString;
        }
    }

    private string MasterConnectionString => _container.GetConnectionString() + ";TrustServerCertificate=True";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var schemaPath = Path.Combine(FindRepoRoot(), "db", "MCP_Database_Schema.sql");
        var seedPath = Path.Combine(FindRepoRoot(), "db", "MCP_Database_Seed.sql");

        await ExecuteSqlFileAsync(schemaPath);
        await ExecuteSqlFileAsync(seedPath);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync().AsTask();
    }

    private async Task ExecuteSqlFileAsync(string filePath)
    {
        var sql = await File.ReadAllTextAsync(filePath);

        // Split on GO statements (SQL Server batch separator)
        var batches = System.Text.RegularExpressions.Regex.Split(sql, @"^\s*GO\s*$",
            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        await using var connection = new SqlConnection(MasterConnectionString);
        await connection.OpenAsync();

        foreach (var batch in batches)
        {
            var trimmed = batch.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            await using var cmd = new SqlCommand(trimmed, connection);
            cmd.CommandTimeout = 60;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir, "db")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not find repo root with 'db' folder.");
    }
}

[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>;
