using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class AuditLog
{
    public int LogId { get; set; }

    public string TableName { get; set; } = null!;

    public string Action { get; set; } = null!;

    public int RecordId { get; set; }

    public int? PerformedBy { get; set; }

    public DateTime TimeStamp { get; set; }
}
