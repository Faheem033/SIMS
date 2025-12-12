using System;

namespace SIMS.BLL.Models
{
    public class MemberDto
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public int RoleId { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }
        public string? PasswordHash { get; set; } 
    }
}