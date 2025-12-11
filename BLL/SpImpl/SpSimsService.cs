using System;
using SIMS.Data;
using System.Data;
using System.Collections.Generic;
using SIMS.BLL.Models;


// I wrote this in the /Data folder, I have the connection string written there. Will prevent multiple string rewrites apart from there
// using IDbConnection conn = SqlConnectionFactory.Create();  This will return the DB conenction (hopefully)

namespace SIMS.BLL.SpImpl
{
    public class SpSimsService : InterfaceSimsService
    {
        public List<MemberDto> GetMembers(string? searchTerm = null) =>
            throw new NotImplementedException();

        public MemberDto? GetMemberById(int memberId) =>
            throw new NotImplementedException();

        public bool CreateMember(MemberDto member) =>
            throw new NotImplementedException();

        public bool UpdateMember(MemberDto member) =>
            throw new NotImplementedException();

        public bool DeactivateMember(int memberId) =>
            throw new NotImplementedException();

        public List<RoleDto> GetRoles() =>
            throw new NotImplementedException();

        public List<EventDto> GetUpcomingEvents() =>
            throw new NotImplementedException();

        public bool CreateEventWithBudget(EventDto evt, int budgetId) =>
            throw new NotImplementedException();

        public bool RegisterMemberForEvent(int memberId, int eventId) =>
            throw new NotImplementedException();

        public List<BudgetDto> GetBudgets() =>
            throw new NotImplementedException();

        public bool AddExpense(ExpenseDto expense) =>
            throw new NotImplementedException();

        public List<EventFinancialSummaryDto> GetEventFinancialSummary() =>
            throw new NotImplementedException();

        public List<BudgetUtilizationDto> GetBudgetUtilizationSummary() =>
            throw new NotImplementedException();

        public bool CreateAnnouncementAndNotify(AnnouncementDto announcement) =>
            throw new NotImplementedException();

        public List<AnnouncementDto> GetRecentAnnouncements(int maxCount = 20) =>
            throw new NotImplementedException();

        public List<NotificationDto> GetNotificationsForMember(int memberId, bool onlyUnread) =>
            throw new NotImplementedException();

        public bool MarkNotificationAsRead(int notificationId) =>
            throw new NotImplementedException();

        public List<MemberParticipationSummaryDto> GetMemberParticipationSummary() =>
            throw new NotImplementedException();

        public List<AttendanceTrendPointDto> GetAttendanceTrend() =>
            throw new NotImplementedException();
    }
}
