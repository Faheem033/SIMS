using System.Collections.Generic;
using SIMS.BLL.Models;

namespace SIMS.BLL
{
    public interface InterfaceSimsService
    {
        //Members
        List<MemberDto> GetMembers(string? searchTerm = null);
        MemberDto? GetMemberById(int memberId);
        bool CreateMember(MemberDto member);
        bool UpdateMember(MemberDto member);
        bool DeactivateMember(int memberId);
        List<RoleDto> GetRoles();


        //Events
        List<EventDto> GetUpcomingEvents();
        bool CreateEventWithBudget(EventDto evt, int budgetId); 
        bool RegisterMemberForEvent(int memberId, int eventId);


        //Budgets
        List<BudgetDto> GetBudgets();
        bool AddExpense(ExpenseDto expense); 
        List<EventFinancialSummaryDto> GetEventFinancialSummary(); 
        List<BudgetUtilizationDto> GetBudgetUtilizationSummary();  


        //Announcements
        bool CreateAnnouncementAndNotify(AnnouncementDto announcement); 
        List<AnnouncementDto> GetRecentAnnouncements(int maxCount = 20);
        List<NotificationDto> GetNotificationsForMember(int memberId, bool onlyUnread);
        bool MarkNotificationAsRead(int notificationId);

        //Analytics
        List<MemberParticipationSummaryDto> GetMemberParticipationSummary(); 
        List<AttendanceTrendPointDto> GetAttendanceTrend();              
    }
}
