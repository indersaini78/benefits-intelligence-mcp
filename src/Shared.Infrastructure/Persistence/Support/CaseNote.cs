using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

public partial class CaseNote
{
    public long CaseNoteId { get; set; }

    public string CaseId { get; set; } = null!;

    public string AuthorUserId { get; set; } = null!;

    public string AuthorType { get; set; } = null!;

    public string NoteText { get; set; } = null!;

    public bool IsInternal { get; set; }

    public DateTime CreatedUtc { get; set; }

    public virtual Case Case { get; set; } = null!;
}
