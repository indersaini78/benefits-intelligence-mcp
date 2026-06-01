using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Iam;

public partial class AccessRole
{
    public string RoleId { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public string ScopesJson { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
