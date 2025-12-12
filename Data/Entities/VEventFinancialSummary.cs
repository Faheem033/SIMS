using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class VEventFinancialSummary
{
    public int EventId { get; set; }

    public string EventTitle { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public string OrganizedBy { get; set; } = null!;

    public decimal? TotalAllocated { get; set; }

    public decimal? TotalExpenses { get; set; }

    public decimal? RemainingAmount { get; set; }
}
