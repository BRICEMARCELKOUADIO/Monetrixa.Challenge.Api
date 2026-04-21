using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Monetrixa.ChallengeApp.Infrastructure.Persistence;

namespace Monetrixa.ChallengeApp.Tests.Common;

public static class SqliteTestDbContextFactory
{
    public static (ChallengeDbContext DbContext, SqliteConnection Connection) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ChallengeDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new ChallengeDbContext(options);
        dbContext.Database.EnsureCreated();

        return (dbContext, connection);
    }
}