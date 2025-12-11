namespace SIMS.BLL.Models
{
    public class AttendanceTrendPointDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalAttendance { get; set; }
    }
}
