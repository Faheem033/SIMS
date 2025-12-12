using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class VMemberParticipationSummary
{
    public int MemberId { get; set; }

    public string FullName { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public decimal? AttendanceRate { get; set; }

    public int? TotalRegistered { get; set; }

    public int? TotalAttended { get; set; }

    public int? TotalCancelled { get; set; }
}
