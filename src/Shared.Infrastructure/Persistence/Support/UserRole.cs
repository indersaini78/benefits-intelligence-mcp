using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

public partial class UserRole
{
    public string UserId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public DateTime GrantedUtc { get; set; }

    public string GrantedBy { get; set; } = null!;

    public virtual AccessRole Role { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
