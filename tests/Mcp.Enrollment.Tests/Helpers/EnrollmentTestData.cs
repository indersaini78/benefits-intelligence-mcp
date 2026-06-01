using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Enrollment;

namespace Mcp.Enrollment.Tests.Helpers;

public sealed class EnrollmentTestData
{
    private readonly EnrollmentDbContext _db;

    private EnrollmentTestData(EnrollmentDbContext db) => _db = db;

    public static EnrollmentTestData For(EnrollmentDbContext db) => new(db);

    public EnrollmentTestData WithMember(string memberId, string groupPolicyId, DateOnly hireDate)
    {
        _db.Members.Add(new Member
        {
            MemberId = memberId,
            GroupPolicyId = groupPolicyId,
            FirstName = "Test",
            LastName = "User",
            DOB = new DateOnly(1990, 1, 1),
            EmploymentStatus = "Active",
            HireDate = hireDate,
            CreatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public EnrollmentTestData WithCoverage(string memberId, string planId, string tier)
    {
        _db.MemberCoverages.Add(new MemberCoverage
        {
            CoverageId = $"COV-{Guid.NewGuid():N}"[..20],
            MemberId = memberId,
            PlanId = planId,
            Tier = tier,
            EffectiveDate = new DateOnly(2024, 1, 1),
            Status = "Active"
        });
        return this;
    }

    public EnrollmentTestData WithDependent(string memberId, string firstName, string lastName, string relationship, DateOnly dob)
    {
        _db.Dependents.Add(new Dependent
        {
            DependentId = $"DEP-{Guid.NewGuid():N}"[..20],
            MemberId = memberId,
            FirstName = firstName,
            LastName = lastName,
            Relationship = relationship,
            DOB = dob,
            IsActive = true
        });
        return this;
    }

    public EnrollmentTestData WithEnrollmentTransaction(string enrollmentId, string memberId, string transactionType, string status)
    {
        _db.EnrollmentTransactions.Add(new EnrollmentTransaction
        {
            EnrollmentId = enrollmentId,
            MemberId = memberId,
            GroupPolicyId = "DEFAULT",
            TransactionType = transactionType,
            Status = status,
            EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
            SubmittedBy = "MCP-Agent",
            SubmittedChannel = "MCP",
            RequestPayloadJson = "{}",
            CreatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public async Task SeedAsync() => await _db.SaveChangesAsync();

    public static EnrollmentDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EnrollmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EnrollmentDbContext(options);
    }
}
