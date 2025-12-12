using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Budget
{
    public int BudgetId { get; set; }

    public string BudgetName { get; set; } = null!;

    public decimal TotalFunds { get; set; }

    public decimal FundsUsed { get; set; }

    public decimal? RemainingFunds { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual EventBudget? EventBudget { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
