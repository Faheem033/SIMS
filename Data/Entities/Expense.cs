using System;
using System.Collections.Generic;

namespace SIMS.Data.Entities;

public partial class Expense
{
    public int ExpenseId { get; set; }

    public int BudgetId { get; set; }

    public string Title { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }

    public int CreatedBy { get; set; }

    public virtual Budget Budget { get; set; } = null!;

    public virtual Member CreatedByNavigation { get; set; } = null!;
}
