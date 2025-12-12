using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Announcement
{
    public int AnnouncementId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int? EventId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member CreatedByNavigation { get; set; } = null!;

    public virtual Event? Event { get; set; }
}
