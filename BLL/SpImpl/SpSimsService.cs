using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient; 
using SIMS.BLL.Models;
using SIMS.Data;

namespace SIMS.BLL.SpImpl
{
    public class SpSimsService : InterfaceSimsService
    {
        private SqlConnection GetConnection()
        {
            var conn = (SqlConnection)SqlConnectionFactory.Create();
            if (conn.State != ConnectionState.Open) conn.Open();
            return conn;
        }

        public List<MemberDto> GetMembers(string? searchTerm = null)
        {
            var list = new List<MemberDto>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;

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

        public List<EventDto> GetUpcomingEvents()
        {
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
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool RegisterMemberForEvent(int memberId, int eventId)
        {
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

        public bool CreateAnnouncementAndNotify(AnnouncementDto announcement)
        {
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
        public List<MemberParticipationSummaryDto> GetMemberParticipationSummary()
        {
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