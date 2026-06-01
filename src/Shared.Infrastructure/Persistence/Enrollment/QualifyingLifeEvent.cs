using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Enrollment;

public partial class QualifyingLifeEvent
{
    public string QleId { get; set; } = null!;

    public string MemberId { get; set; } = null!;

    public string QleType { get; set; } = null!;

    public DateOnly QleDate { get; set; }

    public DateOnly ElectionWindowEnd { get; set; }

    public string Status { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
