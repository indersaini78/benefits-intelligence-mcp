using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Support;

namespace Mcp.CustomerSupport.Tests.Helpers;

public sealed class SupportTestData
{
    private readonly SupportDbContext _db;

    private SupportTestData(SupportDbContext db) => _db = db;

    public static SupportTestData For(SupportDbContext db) => new(db);

    public SupportTestData WithCase(string caseId, string? memberId, string caseType, string status,
        string priority = "Medium", string channel = "Portal", string? assignedQueue = null,
        string createdBy = "USR-AGENT", string createdBySource = "Agent")
    {
        var now = DateTime.UtcNow;
        _db.Cases.Add(new Case
        {
            CaseId = caseId,
            MemberId = memberId,
            CaseType = caseType,
            Subject = $"Test case {caseId}",
            Status = status,
            Priority = priority,
            Channel = channel,
            AssignedQueue = assignedQueue,
            CreatedByUserId = createdBy,
            CreatedBySource = createdBySource,
            CreatedUtc = now,
            UpdatedUtc = now
        });
        return this;
    }

    public SupportTestData WithNote(string caseId, string authorType = "Agent", string noteText = "Test note", bool isInternal = true)
    {
        _db.CaseNotes.Add(new CaseNote
        {
            CaseId = caseId,
            AuthorUserId = "USR-001",
            AuthorType = authorType,
            NoteText = noteText,
            IsInternal = isInternal,
            CreatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public SupportTestData WithInteraction(string? caseId, string? memberId = "MEM-001",
        string channel = "Phone", string direction = "Inbound")
    {
        _db.Interactions.Add(new Interaction
        {
            InteractionId = $"INT-{Guid.NewGuid():N}"[..20],
            MemberId = memberId,
            CaseId = caseId,
            Channel = channel,
            Direction = direction,
            HandledByAgent = true,
            StartedUtc = DateTime.UtcNow
        });
        return this;
    }

    public SupportTestData WithEscalation(string caseId, string toQueue = "Tier2", string reason = "LowConfidence")
    {
        _db.Escalations.Add(new Escalation
        {
            EscalationId = $"ESC-{Guid.NewGuid():N}"[..20],
            CaseId = caseId,
            ToQueue = toQueue,
            Reason = reason,
            Status = "Pending",
            EscalatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public SupportTestData WithUser(string userId, string username)
    {
        _db.UserAccounts.Add(new UserAccount
        {
            UserId = userId,
            Username = username,
            Email = $"{username}@test.com",
            UserType = "CSR",
            Status = "Active",
            CreatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public async Task SeedAsync() => await _db.SaveChangesAsync();

    public static SupportDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SupportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SupportDbContext(options);
    }
}
