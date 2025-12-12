using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int ParticipationId { get; set; }

    public DateTime CheckInTime { get; set; }

    public virtual Participation Participation { get; set; } = null!;
}
