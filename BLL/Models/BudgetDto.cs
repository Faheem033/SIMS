using System;

namespace SIMS.BLL.Models
{
    public class BudgetDto
    {
        public int BudgetId { get; set; }
        public string BudgetName { get; set; } = "";
        public decimal TotalFunds { get; set; }
        public decimal FundsUsed { get; set; }
        public decimal RemainingFunds { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
