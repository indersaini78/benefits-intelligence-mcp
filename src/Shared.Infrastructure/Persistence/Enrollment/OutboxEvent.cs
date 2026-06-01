using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Enrollment;

public partial class OutboxEvent
{
    public long OutboxId { get; set; }

    public string AggregateId { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public string PayloadJson { get; set; } = null!;

    public DateTime CreatedUtc { get; set; }

    public DateTime? PublishedUtc { get; set; }
}
