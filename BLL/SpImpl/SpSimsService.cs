// using System;
// using SIMS.Data;
// using System.Data;
// using System.Collections.Generic;
// using SIMS.BLL.Models;


// // I wrote this in the /Data folder, I have the connection string written there. Will prevent multiple string rewrites apart from there
// // using IDbConnection conn = SqlConnectionFactory.Create();  This will return the DB conenction (hopefully)

// namespace SIMS.BLL.SpImpl
// {
//     public class SpSimsService : InterfaceSimsService
//     {
//         public List<MemberDto> GetMembers(string? searchTerm = null) =>
//             throw new NotImplementedException();

//         public MemberDto? GetMemberById(int memberId) =>
//             throw new NotImplementedException();

//         public bool CreateMember(MemberDto member) =>
//             throw new NotImplementedException();

//         public bool UpdateMember(MemberDto member) =>
//             throw new NotImplementedException();

//         public bool DeactivateMember(int memberId) =>
//             throw new NotImplementedException();

//         public List<RoleDto> GetRoles() =>
//             throw new NotImplementedException();

//         public List<EventDto> GetUpcomingEvents() =>
//             throw new NotImplementedException();

//         public bool CreateEventWithBudget(EventDto evt, int budgetId) =>
//             throw new NotImplementedException();

//         public bool RegisterMemberForEvent(int memberId, int eventId) =>
//             throw new NotImplementedException();

//         public List<BudgetDto> GetBudgets() =>
//             throw new NotImplementedException();

//         public bool AddExpense(ExpenseDto expense) =>
//             throw new NotImplementedException();

//         public List<EventFinancialSummaryDto> GetEventFinancialSummary() =>
//             throw new NotImplementedException();

//         public List<BudgetUtilizationDto> GetBudgetUtilizationSummary() =>
//             throw new NotImplementedException();

//         public bool CreateAnnouncementAndNotify(AnnouncementDto announcement) =>
//             throw new NotImplementedException();

//         public List<AnnouncementDto> GetRecentAnnouncements(int maxCount = 20) =>
//             throw new NotImplementedException();

//         public List<NotificationDto> GetNotificationsForMember(int memberId, bool onlyUnread) =>
//             throw new NotImplementedException();

//         public bool MarkNotificationAsRead(int notificationId) =>
//             throw new NotImplementedException();

//         public List<MemberParticipationSummaryDto> GetMemberParticipationSummary() =>
//             throw new NotImplementedException();

//         public List<AttendanceTrendPointDto> GetAttendanceTrend() =>
//             throw new NotImplementedException();
//     }
// }


using System;
using SIMS.Data;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using SIMS.BLL.Models;

namespace SIMS.BLL.SpImpl
{
    public class SpSimsService : InterfaceSimsService
    {
        // Helper to get open connection from Member 1's Factory
        private SqlConnection GetConnection()
        {
            // We cast IDbConnection to SqlConnection because we need specific SQL capabilities
            var conn = (SqlConnection)SqlConnectionFactory.Create();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        // =============================================================
        // MEMBER OPERATIONS
        // =============================================================

        public List<MemberDto> GetMembers(string? searchTerm = null)
        {
            var list = new List<MemberDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                string sql = "SELECT m.*, r.RoleName FROM ems.Member m JOIN ems.Role r ON m.RoleID = r.RoleID";
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    sql += " WHERE m.FullName LIKE @Search OR m.Email LIKE @Search";
                    cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                }

                cmd.CommandText = sql;
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapMember(reader));
                    }
                }
            }
            return list;
        }

        public MemberDto? GetMemberById(int memberId)
        {
            // REQUIREMENT: Call fn_GetMemberAttendanceRate here
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                // We call the Scalar Function inline
                cmd.CommandText = @"
                    SELECT m.*, r.RoleName, 
                           ems.fn_GetMemberAttendanceRate(m.MemberID) AS AttendanceRate
                    FROM ems.Member m 
                    JOIN ems.Role r ON m.RoleID = r.RoleID
                    WHERE m.MemberID = @Id";

                cmd.Parameters.AddWithValue("@Id", memberId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var member = MapMember(reader);
                        // Assuming DTO has an AttendanceRate property (decimal or double)
                        // If not, you might need to add it to the DTO
                        if (HasColumn(reader, "AttendanceRate"))
                             // Store this if MemberDto has a field for it, otherwise ignore
                             // member.AttendanceRate = Convert.ToDecimal(reader["AttendanceRate"]);
                        
                        return member;
                    }
                }
            }
            return null;
        }

        public bool CreateMember(MemberDto member)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("INSERT INTO ems.Member (FullName, Email, PhoneNumber, RoleID, PasswordHash) VALUES (@Name, @Email, @Phone, @Role, @Pass)", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", member.FullName);
                    cmd.Parameters.AddWithValue("@Email", member.Email);
                    cmd.Parameters.AddWithValue("@Phone", member.PhoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Role", member.RoleId);
                    cmd.Parameters.AddWithValue("@Pass", "default_hash"); // Simplified for assignment
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public bool UpdateMember(MemberDto member)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("UPDATE ems.Member SET FullName=@Name, PhoneNumber=@Phone, RoleID=@Role WHERE MemberID=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", member.FullName);
                    cmd.Parameters.AddWithValue("@Phone", member.PhoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Role", member.RoleId);
                    cmd.Parameters.AddWithValue("@Id", member.MemberId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public bool DeactivateMember(int memberId)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("UPDATE ems.Member SET IsActive = 0 WHERE MemberID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", memberId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public List<RoleDto> GetRoles()
        {
            var list = new List<RoleDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT * FROM ems.Role", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new RoleDto 
                    { 
                        RoleId = (int)reader["RoleID"], 
                        RoleName = (string)reader["RoleName"] 
                    });
                }
            }
            return list;
        }

        // =============================================================
        // EVENT OPERATIONS (MANDATORY STORED PROCEDURES)
        // =============================================================

        public List<EventDto> GetUpcomingEvents()
        {
            // REQUIREMENT: Use View v_UpcomingEvents
            var list = new List<EventDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT * FROM ems.v_UpcomingEvents", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new EventDto
                    {
                        EventId = (int)reader["EventID"],
                        Title = (string)reader["Title"],
                        Description = (string)reader["Description"],
                        EventDate = (DateTime)reader["EventDate"],
                        Venue = (string)reader["Venue"],
                        OrganizedByMemberId = (int)reader["OrganizedBy"],
                        BudgetAllocated = (int)reader["BudgetAllocated"],
                        Capacity = (int)reader["Capacity"],
                    });
                }
            }
            return list;
        }

        public bool CreateEventWithBudget(EventDto evt, int budgetId)
        {
            // REQUIREMENT: Call sp_CreateEventWithBudget
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("ems.sp_CreateEventWithBudget", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Title", evt.Title);
                    cmd.Parameters.AddWithValue("@Description", evt.Description ?? "");
                    cmd.Parameters.AddWithValue("@EventDate", evt.EventDate);
                    cmd.Parameters.AddWithValue("@Venue", evt.Venue);
                    cmd.Parameters.AddWithValue("@OrganizedBy", evt.OrganizedByMemberId);
                    cmd.Parameters.AddWithValue("@Capacity", evt.Capacity);
                    cmd.Parameters.AddWithValue("@BudgetID", budgetId);
                    // Assuming DTO has BudgetAllocated, otherwise pass it separately
                    cmd.Parameters.AddWithValue("@AmountAllocated", evt.BudgetAllocated);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqlException ex)
            {
                // Logic error (e.g. Insufficient Funds) will be caught here
                Console.WriteLine("SP Error: " + ex.Message);
                return false; 
            }
        }

        public bool RegisterMemberForEvent(int memberId, int eventId)
        {
            // REQUIREMENT: Call sp_RegisterMemberForEvent
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("ems.sp_RegisterMemberForEvent", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    cmd.Parameters.AddWithValue("@MemberID", memberId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Registration Error: " + ex.Message);
                return false;
            }
        }

        // =============================================================
        // BUDGET & EXPENSE OPERATIONS
        // =============================================================

        public List<BudgetDto> GetBudgets()
        {
            var list = new List<BudgetDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT * FROM ems.Budget", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new BudgetDto
                    {
                        BudgetId = (int)reader["BudgetID"],
                        BudgetName = (string)reader["BudgetName"],
                        TotalFunds = (decimal)reader["TotalFunds"],
                        FundsUsed = (decimal)reader["FundsUsed"],
                        // RemainingFunds is calculated column, can fetch or calc in C#
                        RemainingFunds = (decimal)reader["TotalFunds"] - (decimal)reader["FundsUsed"]
                    });
                }
            }
            return list;
        }

        public bool AddExpense(ExpenseDto expense)
        {
            // REQUIREMENT: Call sp_AddExpenseWithBudgetCheck
            // REQUIREMENT: This implicitly tests tr_Expense_AfterInsert
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("ems.sp_AddExpenseWithBudgetCheck", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BudgetID", expense.BudgetId);
                    cmd.Parameters.AddWithValue("@Title", expense.Title);
                    cmd.Parameters.AddWithValue("@Amount", expense.Amount);
                    cmd.Parameters.AddWithValue("@CreatedBy", expense.CreatedByMemberId);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Expense Error: " + ex.Message);
                return false;
            }
        }

        public List<EventFinancialSummaryDto> GetEventFinancialSummary()
        {
            // REQUIREMENT: Use View v_EventFinancialSummary
            var list = new List<EventFinancialSummaryDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT * FROM ems.v_EventFinancialSummary", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new EventFinancialSummaryDto
                    {
                        EventId = (int)reader["EventID"],
                        EventTitle = (string)reader["EventTitle"],
                        TotalAllocated = (decimal)reader["TotalAllocated"],
                        TotalExpenses = (decimal)reader["TotalExpenses"],
                        RemainingAmount = (decimal)reader["RemainingAmount"]
                    });
                }
            }
            return list;
        }

        public List<BudgetUtilizationDto> GetBudgetUtilizationSummary()
        {
            // REQUIREMENT: Use CTE/SP sp_GetBudgetUtilizationSummary
            var list = new List<BudgetUtilizationDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("ems.sp_GetBudgetUtilizationSummary", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BudgetUtilizationDto
                        {
                            BudgetId = (int)reader["BudgetID"],
                            BudgetName = (string)reader["BudgetName"],
                            UsagePercent = (decimal)reader["UsagePercent"],
                            RiskLevel = (string)reader["RiskLevel"]
                        });
                    }
                }
            }
            return list;
        }

        // =============================================================
        // ANNOUNCEMENTS & NOTIFICATIONS
        // =============================================================

        public bool CreateAnnouncementAndNotify(AnnouncementDto announcement)
        {
            // REQUIREMENT: Call sp_CreateAnnouncementAndNotify
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("ems.sp_CreateAnnouncementAndNotify", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Title", announcement.Title);
                    cmd.Parameters.AddWithValue("@Message", announcement.Message);
                    // Handle nullable EventID
                    cmd.Parameters.AddWithValue("@EventID", announcement.EventId.HasValue ? (object)announcement.EventId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", announcement.CreatedByMemberId);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public List<AnnouncementDto> GetRecentAnnouncements(int maxCount = 20)
        {
            var list = new List<AnnouncementDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"SELECT TOP {maxCount} * FROM ems.Announcement ORDER BY CreatedAt DESC", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new AnnouncementDto
                    {
                        AnnouncementId = (int)reader["AnnouncementID"],
                        Title = (string)reader["Title"],
                        Message = (string)reader["Message"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }
            return list;
        }

        public List<NotificationDto> GetNotificationsForMember(int memberId, bool onlyUnread)
        {
            var list = new List<NotificationDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                string sql = "SELECT * FROM ems.Notification WHERE MemberID = @MemberID";
                if (onlyUnread) sql += " AND IsRead = 0";
                sql += " ORDER BY CreatedAt DESC";
                
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@MemberID", memberId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new NotificationDto
                        {
                            NotificationId = (int)reader["NotificationID"],
                            Message = (string)reader["Message"],
                            IsRead = (bool)reader["IsRead"],
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        });
                    }
                }
            }
            return list;
        }

        public bool MarkNotificationAsRead(int notificationId)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("UPDATE ems.Notification SET IsRead = 1 WHERE NotificationID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", notificationId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        // =============================================================
        // ANALYTICS (VIEWS & CTEs)
        // =============================================================

        public List<MemberParticipationSummaryDto> GetMemberParticipationSummary()
        {
            // REQUIREMENT: Use View v_MemberParticipationSummary
            var list = new List<MemberParticipationSummaryDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT * FROM ems.v_MemberParticipationSummary", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new MemberParticipationSummaryDto
                    {
                        MemberId = (int)reader["MemberID"],
                        FullName = (string)reader["FullName"],
                        RoleName = (string)reader["RoleName"],
                        AttendanceRate = (decimal)reader["AttendanceRate"],
                        TotalRegistered = (int)reader["TotalRegistered"],
                        TotalAttended = (int)reader["TotalAttended"]
                    });
                }
            }
            return list;
        }

        public List<AttendanceTrendPointDto> GetAttendanceTrend()
        {
            // REQUIREMENT: Use CTE/SP sp_GetAttendanceTrend
            var list = new List<AttendanceTrendPointDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("ems.sp_GetAttendanceTrend", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AttendanceTrendPointDto
                        {
                            EventTitle = (string)reader["EventTitle"],
                            Year = (int)reader["Year"],
                            Month = (int)reader["Month"],
                            TotalAttendance = (int)reader["TotalAttendance"]
                        });
                    }
                }
            }
            return list;
        }

        // =============================================================
        // HELPERS
        // =============================================================

        private MemberDto MapMember(SqlDataReader reader)
        {
            return new MemberDto
            {
                MemberId = (int)reader["MemberID"],
                FullName = (string)reader["FullName"],
                Email = (string)reader["Email"],
                PhoneNumber = reader["PhoneNumber"] as string,
                RoleId = (int)reader["RoleID"],
                JoinDate = (DateTime)reader["JoinDate"],
                PasswordHash = (string)reader["PasswordHash"],
                IsActive = (bool)reader["IsActive"]
            };
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}