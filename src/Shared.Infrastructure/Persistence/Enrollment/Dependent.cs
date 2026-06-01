using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Enrollment;

public partial class Dependent
{
    public string DependentId { get; set; } = null!;

    public string MemberId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Relationship { get; set; } = null!;

    public DateOnly DOB { get; set; }

    public string? Gender { get; set; }

    public bool IsActive { get; set; }

    public virtual Member Member { get; set; } = null!;
}
