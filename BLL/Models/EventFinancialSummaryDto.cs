using System;

namespace SIMS.BLL.Models
{
    public class EventFinancialSummaryDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string OrganizedBy { get; set; } = "";
        public decimal TotalAllocated { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
