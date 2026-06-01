using BenefitsIntelligence.IntegrationTests.Fixtures;
using FluentAssertions;
using Mcp.AccountAccess.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.AccountAccess;
using Shared.Infrastructure.Persistence.Iam;

namespace BenefitsIntelligence.IntegrationTests;

[Collection("SqlServer")]
public sealed class AccountAccessWriteTests(SqlServerFixture fixture)
{
    private IamDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IamDbContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;
        return new IamDbContext(options);
    }

    [Fact]
    public async Task UnlockAccountAsync_UnlocksLockedUser()
    {
        // Arrange - MEM-10004's user account USR-MEM-10004 is seeded as Locked
        await using var db = CreateDbContext();
        var handler = new UnlockAccountHandler(db);

        var request = new UnlockAccountRequest("USR-MEM-10004", "Agent verified member identity via phone");
        var command = new UnlockAccountCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Active");
        result.UserId.Should().Be("USR-MEM-10004");

        await using var verifyDb = CreateDbContext();
        var user = await verifyDb.UserAccounts.FirstAsync(u => u.UserId == "USR-MEM-10004");
        user.Status.Should().Be("Active");
        user.FailedLoginCount.Should().Be(0);
        user.LockedUntilUtc.Should().BeNull();
    }
}
