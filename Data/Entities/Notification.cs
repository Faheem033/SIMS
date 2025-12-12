using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int MemberId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member Member { get; set; } = null!;
}
