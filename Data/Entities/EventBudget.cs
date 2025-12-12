using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class EventBudget
{
    public int EventBudgetId { get; set; }

    public int EventId { get; set; }

    public int BudgetId { get; set; }

    public decimal AmountAllocated { get; set; }

    public virtual Budget Budget { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
