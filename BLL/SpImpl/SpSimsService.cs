using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient; // Using the newer library
using SIMS.BLL.Models;
using SIMS.Data;

namespace SIMS.BLL.SpImpl
{
    public class SpSimsService : InterfaceSimsService
    {
        // 1. Connection Helper
        // We create a new connection every time to take advantage of "Connection Pooling".
        // It's like borrowing a book from a library (Pool) and returning it immediately.
        private SqlConnection GetConnection()
        {
            var conn = (SqlConnection)SqlConnectionFactory.Create();
            if (conn.State != ConnectionState.Open) conn.Open();
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

                // LINQ Equivalent: _context.Members.AsQueryable() ... Where(...)
                string sql = "SELECT * FROM ems.Member";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    sql += " WHERE FullName LIKE @Search OR Email LIKE @Search";
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
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT * FROM ems.Member WHERE MemberID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", memberId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapMember(reader);
                    }
                }
            }
            return null;
        }

        public bool CreateMember(MemberDto member)
        {
            // LINQ Equivalent: .Add(entity) -> .SaveChanges()
            // No SP exists for creation, so we use inline SQL.
            try
            {
                using (var conn = GetConnection())
                {
                    string sql = @"INSERT INTO ems.Member (FullName, Email, PhoneNumber, RoleID, JoinDate, PasswordHash, IsActive) 
                                   VALUES (@Name, @Email, @Phone, @Role, @Join, @Pass, @Active)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", member.FullName);
                        cmd.Parameters.AddWithValue("@Email", member.Email);
                        cmd.Parameters.AddWithValue("@Phone", member.PhoneNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Role", member.RoleId);
                        cmd.Parameters.AddWithValue("@Join", member.JoinDate);
                        cmd.Parameters.AddWithValue("@Pass", member.PasswordHash ?? "");
                        cmd.Parameters.AddWithValue("@Active", member.IsActive);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool UpdateMember(MemberDto member)
        {
            // LINQ Equivalent: .Update(entity) -> .SaveChanges()
            try
            {
                using (var conn = GetConnection())
                {
                    string sql = @"UPDATE ems.Member 
                                   SET FullName=@Name, Email=@Email, PhoneNumber=@Phone, RoleID=@Role, 
                                       JoinDate=@Join, PasswordHash=@Pass, IsActive=@Active 
                                   WHERE MemberID=@Id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", member.FullName);
                        cmd.Parameters.AddWithValue("@Email", member.Email);
                        cmd.Parameters.AddWithValue("@Phone", member.PhoneNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Role", member.RoleId);
                        cmd.Parameters.AddWithValue("@Join", member.JoinDate);
                        cmd.Parameters.AddWithValue("@Pass", member.PasswordHash ?? "");
                        cmd.Parameters.AddWithValue("@Active", member.IsActive);
                        cmd.Parameters.AddWithValue("@Id", member.MemberId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
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
                    return cmd.ExecuteNonQuery() > 0;
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
                        RoleName = (string)reader["RoleName"],
                        Description = reader["Description"] as string
                    });
                }
            }
            return list;
        }

        // =============================================================
        // EVENT OPERATIONS
        // =============================================================

        public List<EventDto> GetUpcomingEvents()
        {
            // LINQ Equivalent: _context.Events.Where(e => e.EventDate >= today)
            // NOTE: We cannot use View `v_UpcomingEvents` because LINQ selects 
            // BudgetAllocated and Capacity, but the View does not return those.
            // We must use raw SQL to match LINQ logic perfectly.

            var list = new List<EventDto>();
            using (var conn = GetConnection())
            {
                string sql = @"SELECT * FROM ems.[Event] WHERE EventDate >= SYSDATETIME()";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new EventDto
                        {
                            EventId = (int)reader["EventID"],
                            Title = (string)reader["Title"],
                            Description = reader["Description"] as string,
                            EventDate = (DateTime)reader["EventDate"],
                            Venue = (string)reader["Venue"],
                            OrganizedByMemberId = (int)reader["OrganizedBy"], // Matched LINQ property name
                            BudgetAllocated = (decimal)reader["BudgetAllocated"],
                            Capacity = (int)reader["Capacity"]
                        });
                    }
                }
            }
            return list;
        }

        public bool CreateEventWithBudget(EventDto evt, int budgetId)
        {
            // LINQ Equivalent: Uses Transaction + Logic
            // SP Equivalent: ems.sp_CreateEventWithBudget (Contains logic + transaction)
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
                    cmd.Parameters.AddWithValue("@AmountAllocated", evt.BudgetAllocated);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqlException ex)
            {
                // SP raises error if funds are insufficient, returning false like LINQ
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool RegisterMemberForEvent(int memberId, int eventId)
        {
            // LINQ Equivalent: Checks existence, Adds Participation
            // SP Equivalent: ems.sp_RegisterMemberForEvent (Checks existence + capacity, then inserts)
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
                Console.WriteLine(ex.Message);
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
                        RemainingFunds = (decimal)reader["RemainingFunds"], // Calculated in DB
                        LastUpdated = (DateTime)reader["LastUpdated"]
                    });
                }
            }
            return list;
        }

        public bool AddExpense(ExpenseDto expense)
        {
            // LINQ Equivalent: Transaction + Update Budget manually
            // SP Equivalent: ems.sp_AddExpenseWithBudgetCheck (Checks logic) 
            //                + ems.tr_Expense_AfterInsert (Updates Budget automatically)
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
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public List<EventFinancialSummaryDto> GetEventFinancialSummary()
        {
            // LINQ uses View -> We use View
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
                        EventDate = (DateTime)reader["EventDate"],
                        OrganizedBy = (string)reader["OrganizedBy"],
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
            // LINQ calculates RiskLevel in C#
            // SP Equivalent: ems.sp_GetBudgetUtilizationSummary (Calculates RiskLevel in SQL)
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
                            TotalFunds = (decimal)reader["TotalFunds"],
                            FundsUsed = (decimal)reader["FundsUsed"],
                            RemainingFunds = (decimal)reader["RemainingFunds"],
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
            // LINQ: Transaction, Insert Announcement, Iterate Members -> Insert Notifications
            // SP: ems.sp_CreateAnnouncementAndNotify (Does exactly this in SQL Set-based op)
            try
            {
                using (var conn = GetConnection())
                using (var cmd = new SqlCommand("ems.sp_CreateAnnouncementAndNotify", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Title", announcement.Title);
                    cmd.Parameters.AddWithValue("@Message", announcement.Message);
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
            // LINQ: OrderByDescending + Take
            var list = new List<AnnouncementDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"SELECT TOP (@Count) * FROM ems.Announcement ORDER BY CreatedAt DESC", conn))
            {
                cmd.Parameters.AddWithValue("@Count", maxCount);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AnnouncementDto
                        {
                            AnnouncementId = (int)reader["AnnouncementID"],
                            Title = (string)reader["Title"],
                            Message = (string)reader["Message"],
                            EventId = reader["EventID"] as int?,
                            CreatedByMemberId = (int)reader["CreatedBy"],
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        });
                    }
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

                // Added Order By to match logical expectation (newest first), though LINQ didn't explicitly specify order
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
                            MemberId = (int)reader["MemberID"],
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
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch { return false; }
        }

        // =============================================================
        // ANALYTICS
        // =============================================================

        public List<MemberParticipationSummaryDto> GetMemberParticipationSummary()
        {
            // LINQ uses View -> We use View
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
                        TotalAttended = (int)reader["TotalAttended"],
                        TotalCancelled = (int)reader["TotalCancelled"]
                    });
                }
            }
            return list;
        }

        public List<AttendanceTrendPointDto> GetAttendanceTrend()
        {
            // LINQ: Does GroupBy in Memory
            // SP: ems.sp_GetAttendanceTrend (Does GroupBy in SQL)
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
                            EventId = (int)reader["EventID"],
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

        // Helper to map Member DataReader to Object cleanly
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
                IsActive = (bool)reader["IsActive"],
                PasswordHash = (string)reader["PasswordHash"]
            };
        }
    }
}