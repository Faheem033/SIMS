using System;

namespace SIMS.BLL.Models
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; } = "";
        public int OrganizedByMemberId { get; set; }   
        public decimal BudgetAllocated { get; set; } 
        public int Capacity { get; set; }           
    }
}
