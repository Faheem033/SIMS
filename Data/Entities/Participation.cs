using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Participation
{
    public int ParticipationId { get; set; }

    public int EventId { get; set; }

    public int MemberId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Event Event { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
