using System;

namespace SIMS.BLL.Models
{
    public class AnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public int? EventId { get; set; }
        public int CreatedByMemberId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
