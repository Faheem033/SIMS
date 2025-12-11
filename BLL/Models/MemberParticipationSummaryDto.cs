namespace SIMS.BLL.Models
{
    public class MemberParticipationSummaryDto
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = "";
        public string RoleName { get; set; } = "";
        public decimal AttendanceRate { get; set; }
        public int TotalRegistered { get; set; }
        public int TotalAttended { get; set; }
        public int TotalCancelled { get; set; }
    }
}
