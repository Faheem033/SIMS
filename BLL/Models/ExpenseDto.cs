using System;

namespace SIMS.BLL.Models
{
    public class ExpenseDto
    {
        public int ExpenseId { get; set; }
        public int BudgetId { get; set; }
        public string Title { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int CreatedByMemberId { get; set; }
    }
}
