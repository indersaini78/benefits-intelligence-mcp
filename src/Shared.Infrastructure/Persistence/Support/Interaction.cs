using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

public partial class Interaction
{
    public string InteractionId { get; set; } = null!;

    public string? MemberId { get; set; }

    public string? CaseId { get; set; }

    public string Channel { get; set; } = null!;

    public string Direction { get; set; } = null!;

    public string? HandledByUserId { get; set; }

    public bool HandledByAgent { get; set; }

    public string? SessionId { get; set; }

    public string? Intent { get; set; }

    public string? Summary { get; set; }

    public string? TranscriptUri { get; set; }

    public decimal? SentimentScore { get; set; }

    public int? DurationSeconds { get; set; }

    public DateTime StartedUtc { get; set; }

    public DateTime? EndedUtc { get; set; }

    public virtual Case? Case { get; set; }

    public virtual UserAccount? HandledByUser { get; set; }

    public virtual Member? Member { get; set; }
}
