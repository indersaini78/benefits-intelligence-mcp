using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Eligibility;

public partial class EmployerGroup
{
    public string GroupPolicyId { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public DateOnly PlanYearStart { get; set; }

    public DateOnly PlanYearEnd { get; set; }

    public string? TpaId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedUtc { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
