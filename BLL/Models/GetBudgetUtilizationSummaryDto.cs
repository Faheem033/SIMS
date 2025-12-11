namespace SIMS.BLL.Models
{
    public class BudgetUtilizationDto
    {
        public int BudgetId { get; set; }
        public string BudgetName { get; set; } = "";
        public decimal TotalFunds { get; set; }
        public decimal FundsUsed { get; set; }
        public decimal RemainingFunds { get; set; }
        public decimal UsagePercent { get; set; }
        public string RiskLevel { get; set; } = "";
    }
}
