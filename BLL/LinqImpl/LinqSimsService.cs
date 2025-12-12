using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SIMS.BLL.Models;
using SIMS.Data.Entities;

namespace SIMS.BLL.LinqImpl
{
    public class LinqSimsService : InterfaceSimsService
    {
        private readonly EventManagementDbContext _context;

        public LinqSimsService(EventManagementDbContext context)
        {
            _context = context;
        }

        public List<MemberDto> GetMembers(string? searchTerm = null)
        {
            var query = _context.Members.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m => m.FullName.Contains(searchTerm) || m.Email.Contains(searchTerm));
            }

            return query.Select(m => new MemberDto
            {
                MemberId = m.MemberId,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                RoleId = m.RoleId,
                JoinDate = m.JoinDate.ToDateTime(TimeOnly.MinValue),
                IsActive = m.IsActive,
                PasswordHash = m.PasswordHash ?? "" 

            }).ToList();
        }

        public MemberDto? GetMemberById(int memberId)
        {
            return _context.Members
                .Where(m => m.MemberId == memberId)
                .Select(m => new MemberDto
                {
                    MemberId = m.MemberId,
                    FullName = m.FullName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    RoleId = m.RoleId,
                    JoinDate = m.JoinDate.ToDateTime(TimeOnly.MinValue),
                    IsActive = m.IsActive,
                    PasswordHash = m.PasswordHash ?? "" 
                })
                .FirstOrDefault();
        }

        public bool CreateMember(MemberDto member)
        {
            var entity = new Member
            {
                FullName = member.FullName,
                Email = member.Email,
                PhoneNumber = member.PhoneNumber,
                RoleId = member.RoleId,
                JoinDate = DateOnly.FromDateTime(member.JoinDate),
                IsActive = member.IsActive,
                PasswordHash = member.PasswordHash ?? "" 
            };

            _context.Members.Add(entity);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateMember(MemberDto member)
        {
            var entity = _context.Members.Find(member.MemberId);
            if (entity == null) return false;

            entity.FullName = member.FullName;
            entity.Email = member.Email;
            entity.PhoneNumber = member.PhoneNumber;
            entity.RoleId = member.RoleId;
            entity.IsActive = member.IsActive;
            entity.JoinDate = DateOnly.FromDateTime(member.JoinDate);
            entity.PasswordHash=member.PasswordHash;
            _context.Members.Update(entity);
            return _context.SaveChanges() > 0;
        }

        public bool DeactivateMember(int memberId)
        {
            var entity = _context.Members.Find(memberId);
            if (entity == null) return false;

            entity.IsActive = false;
            _context.Members.Update(entity);
            return _context.SaveChanges() > 0;
        }

        public List<RoleDto> GetRoles()
        {
            return _context.Roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description
            }).ToList();
        }

        public List<EventDto> GetUpcomingEvents()
        {
            var today = DateTime.Now;
            return _context.Events
                .Where(e => e.EventDate >= today)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Venue = e.Venue,
                    OrganizedByMemberId = e.OrganizedBy,
                    BudgetAllocated = e.BudgetAllocated,
                    Capacity = e.Capacity
                })
                .ToList();
        }

        public bool CreateEventWithBudget(EventDto evt, int budgetId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var budget = _context.Budgets.Find(budgetId);
                if (budget == null || budget.RemainingFunds < evt.BudgetAllocated)
                    return false;

                var entity = new Event
                {
                    Title = evt.Title,
                    Description = evt.Description,
                    EventDate = evt.EventDate,
                    Venue = evt.Venue,
                    OrganizedBy = evt.OrganizedByMemberId,
                    BudgetAllocated = evt.BudgetAllocated,
                    Capacity = evt.Capacity
                };

                _context.Events.Add(entity);

                budget.FundsUsed += evt.BudgetAllocated;
                budget.RemainingFunds = budget.TotalFunds - budget.FundsUsed;
                budget.LastUpdated = DateTime.Now;

                _context.Budgets.Update(budget);
                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool RegisterMemberForEvent(int memberId, int eventId)
        {
            if (_context.Participations.Any(p => p.MemberId == memberId && p.EventId == eventId))
                return false;

            _context.Participations.Add(new Participation
            {
                MemberId = memberId,
                EventId = eventId,
                RegistrationDate = DateTime.Now,
                Status = "Registered"
            });

            return _context.SaveChanges() > 0;
        }

        public List<BudgetDto> GetBudgets()
        {
            return _context.Budgets.Select(b => new BudgetDto
            {
                BudgetId = b.BudgetId,
                BudgetName = b.BudgetName,
                TotalFunds = b.TotalFunds,
                FundsUsed = b.FundsUsed,
                RemainingFunds = b.RemainingFunds ?? (b.TotalFunds - b.FundsUsed),
                LastUpdated = b.LastUpdated
            }).ToList();
        }

        public bool AddExpense(ExpenseDto expense)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var budget = _context.Budgets.Find(expense.BudgetId);
                if (budget == null || budget.RemainingFunds < expense.Amount)
                    return false;

                var entity = new Expense
                {
                    BudgetId = expense.BudgetId,
                    Title = expense.Title,
                    Amount = expense.Amount,
                    ExpenseDate = expense.ExpenseDate,
                    CreatedBy = expense.CreatedByMemberId
                };

                _context.Expenses.Add(entity);
                _context.SaveChanges();  
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public List<EventFinancialSummaryDto> GetEventFinancialSummary()
        {
            return _context.VEventFinancialSummaries.Select(v => new EventFinancialSummaryDto
            {
                EventId = v.EventId,
                EventTitle = v.EventTitle,
                EventDate = v.EventDate,
                OrganizedBy = v.OrganizedBy,
                TotalAllocated = v.TotalAllocated ?? 0m,
                TotalExpenses = v.TotalExpenses ?? 0m,
                RemainingAmount = v.RemainingAmount ?? 0m
            }).ToList();
        }

        public List<BudgetUtilizationDto> GetBudgetUtilizationSummary()
        {
            var budgets = _context.Budgets
                .Select(b => new
                {
                    b.BudgetId,
                    b.BudgetName,
                    b.TotalFunds,
                    b.FundsUsed,
                    RemainingFunds = b.RemainingFunds ?? (b.TotalFunds - b.FundsUsed)
                })
                .AsEnumerable() // move to in-memory to allow statement lambdas
                .Select(b =>
                {
                    var usagePercent = (b.TotalFunds == 0m) ? 0m : b.FundsUsed / b.TotalFunds * 100m;

                    var risk = usagePercent >= 90 ? "High"
                                : usagePercent >= 70 ? "Medium"
                                : "Low";

                    return new BudgetUtilizationDto
                    {
                        BudgetId = b.BudgetId,
                        BudgetName = b.BudgetName,
                        TotalFunds = b.TotalFunds,
                        FundsUsed = b.FundsUsed,
                        RemainingFunds = b.RemainingFunds,
                        UsagePercent = usagePercent,
                        RiskLevel = risk
                    };
                })
                .ToList();

            return budgets;
        }


        public bool CreateAnnouncementAndNotify(AnnouncementDto announcement)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var entity = new Announcement
                {
                    Title = announcement.Title,
                    Message = announcement.Message,
                    EventId = announcement.EventId,
                    CreatedBy = announcement.CreatedByMemberId,
                    CreatedAt = DateTime.Now
                };
                _context.Announcements.Add(entity);

                var members = _context.Members.Where(m => m.IsActive).ToList();
                foreach (var member in members)
                {
                    _context.Notifications.Add(new Notification
                    {
                        MemberId = member.MemberId,
                        Message = announcement.Title,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public List<AnnouncementDto> GetRecentAnnouncements(int maxCount = 20)
        {
            return _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(maxCount)
                .Select(a => new AnnouncementDto
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Message = a.Message,
                    EventId = a.EventId,
                    CreatedByMemberId = a.CreatedBy,
                    CreatedAt = a.CreatedAt
                })
                .ToList();
        }

        public List<NotificationDto> GetNotificationsForMember(int memberId, bool onlyUnread)
        {
            var query = _context.Notifications.Where(n => n.MemberId == memberId);
            if (onlyUnread) query = query.Where(n => !n.IsRead);

            return query.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                MemberId = n.MemberId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public bool MarkNotificationAsRead(int notificationId)
        {
            var entity = _context.Notifications.Find(notificationId);
            if (entity == null) return false;

            entity.IsRead = true;
            _context.Notifications.Update(entity);
            return _context.SaveChanges() > 0;
        }

        public List<MemberParticipationSummaryDto> GetMemberParticipationSummary()
        {
            return _context.VMemberParticipationSummaries.Select(v => new MemberParticipationSummaryDto
            {
                MemberId = v.MemberId,
                FullName = v.FullName,
                RoleName = v.RoleName,
                AttendanceRate = v.AttendanceRate ?? 0m,
                TotalRegistered = v.TotalRegistered ?? 0,
                TotalAttended = v.TotalAttended ?? 0,
                TotalCancelled = v.TotalCancelled ?? 0
            }).ToList();
        }

        public List<AttendanceTrendPointDto> GetAttendanceTrend()
        {
            return _context.Participations
                .Include(p => p.Event)
                .GroupBy(p => new { p.EventId, p.Event.Title, Year = p.RegistrationDate.Year, Month = p.RegistrationDate.Month })
                .Select(g => new AttendanceTrendPointDto
                {
                    EventId = g.Key.EventId,
                    EventTitle = g.Key.Title,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAttendance = g.Count()
                })
                .OrderBy(a => a.Year)
                .ThenBy(a => a.Month)
                .ToList();
        }
    }
}




