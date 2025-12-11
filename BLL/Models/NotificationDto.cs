using System;

namespace SIMS.BLL.Models
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int MemberId { get; set; }
        public string Message { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
