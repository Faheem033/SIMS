// using System;
// using SIMS.BLL;
// using SIMS.BLL.Factory;

// namespace SIMS.UI
// {
//     public static class Menu
//     {
//         public static void Start()
//         {
//             Console.WriteLine("Choose backend mode:");
//             Console.WriteLine("1. LINQ");
//             Console.WriteLine("2. Stored Procedures");
//             Console.Write("Enter choice (1/2): ");

//             var modeChoice = Console.ReadLine();
//             string mode = modeChoice == "2" ? "SP" : "LINQ";

//             InterfaceSimsService service = SimsServiceFactory.Create(mode);

//             while (true)
//             {
//                 Console.WriteLine("\n===== SIMS MAIN MENU =====");
//                 Console.WriteLine("1. List upcoming events");
//                 Console.WriteLine("2. Create event with budget");
//                 Console.WriteLine("3. Add expense");
//                 Console.WriteLine("4. Event financial summary");
//                 Console.WriteLine("5. Member participation summary");
//                 Console.WriteLine("0. Exit");
//                 Console.Write("Select option: ");

//                 var choice = Console.ReadLine();
//                 switch (choice)
//                 {
//                     case "1":
//                         Console.WriteLine("TODO: list upcoming events");
//                         break;
//                     case "2":
//                         Console.WriteLine("TODO: prompt for event fields + budgetId, then call CreateEventWithBudget");
//                         break;
//                     case "3":
//                         Console.WriteLine("TODO: prompt for expense fields, then call AddExpense");
//                         break;
//                     case "4":
//                         Console.WriteLine("TODO: call GetEventFinancialSummary and print results");
//                         break;
//                     case "5":
//                         Console.WriteLine("TODO: call GetMemberParticipationSummary and print");
//                         break;
//                     case "0":
//                         return;
//                     default:
//                         Console.WriteLine("Invalid option");
//                         break;
//                 }
//             }
//         }
//     }
// }

using System;
using SIMS.BLL;
using SIMS.BLL.Factory;
using SIMS.BLL.Models;
using System.Globalization;

namespace SIMS.UI
{
    public static class Menu
    {
        public static void Start()
        {
            Console.WriteLine("Choose backend mode:");
            Console.WriteLine("1. LINQ");
            Console.WriteLine("2. Stored Procedures");
            Console.Write("Enter choice (1/2): ");

            var modeChoice = Console.ReadLine();
            string mode = modeChoice == "2" ? "SP" : "LINQ";

            InterfaceSimsService service = SimsServiceFactory.Create(mode);

            while (true)
            {
                Console.WriteLine("\n===== SIMS MAIN MENU =====");
                Console.WriteLine("1. List upcoming events");
                Console.WriteLine("2. Create event with budget");
                Console.WriteLine("3. Add expense");
                Console.WriteLine("4. Event financial summary");
                Console.WriteLine("5. Member participation summary");
                Console.WriteLine("6. List members");
                Console.WriteLine("7. Create member");
                Console.WriteLine("8. Update member");
                Console.WriteLine("9. Deactivate member");
                Console.WriteLine("10. List roles");
                Console.WriteLine("11. List budgets");
                Console.WriteLine("12. Create announcement");
                Console.WriteLine("13. List recent announcements");
                Console.WriteLine("14. List notifications for member");
                Console.WriteLine("15. Mark notification as read");
                Console.WriteLine("0. Exit");
                Console.Write("Select option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": ListUpcomingEvents(service); break;
                    case "2": CreateEventWithBudget(service); break;
                    case "3": AddExpense(service); break;
                    case "4": ShowEventFinancialSummary(service); break;
                    case "5": ShowMemberParticipationSummary(service); break;
                    case "6": ListMembers(service); break;
                    case "7": CreateMember(service); break;
                    case "8": UpdateMember(service); break;
                    case "9": DeactivateMember(service); break;
                    case "10": ListRoles(service); break;
                    case "11": ListBudgets(service); break;
                    case "12": CreateAnnouncement(service); break;
                    case "13": ListRecentAnnouncements(service); break;
                    case "14": ListNotifications(service); break;
                    case "15": MarkNotificationAsRead(service); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid option"); break;
                }
            }
        }

        // -------------------- Members --------------------
        private static void ListMembers(InterfaceSimsService service)
        {
            var members = service.GetMembers();
            Console.WriteLine("\nMembers:");
            foreach (var m in members)
            {
                Console.WriteLine(
                    $"ID: {m.MemberId}\n" +
                    $"Full Name: {m.FullName}\n" +
                    $"Email: {m.Email}\n" +
                    $"Phone: {m.PhoneNumber}\n" +
                    $"Role ID: {m.RoleId}\n" +
                    $"Join Date: {m.JoinDate:yyyy-MM-dd}\n" +
                    $"Active: {m.IsActive}\n" +
                    $"Password: {m.PasswordHash}\n" +
                    "--------------------------"
                );
            }
        }

        private static bool AuthenticateAdmin(InterfaceSimsService service)
        {
            Console.Write("Enter your Member Id: ");
            int id = int.Parse(Console.ReadLine() ?? "0");

            var member = service.GetMemberById(id);

            if (member == null)
            {
                Console.WriteLine("Invalid member!");
                return false;
            }

            if (member.RoleId != 1 && member.RoleId != 2)
            {
                Console.WriteLine("Access denied! You do not have permission.");
                return false;
            }

            Console.Write("Enter your Password: ");
            string pass = Console.ReadLine() ?? "";

            if (member.PasswordHash != pass)
            {
                Console.WriteLine("Incorrect password!");
                return false;
            }

            return true;
        }

        private static string GeneratePassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }


        private static void CreateMember(InterfaceSimsService service)
        {
            if (!AuthenticateAdmin(service))
                return;

            Console.Write("Full Name: "); string name = Console.ReadLine() ?? "";
            Console.Write("Email: "); string email = Console.ReadLine() ?? "";
            Console.Write("Phone: "); string phone = Console.ReadLine() ?? "";
            Console.Write("Role Id: "); int roleId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Join Date (yyyy-MM-dd): "); 
            DateTime joinDate = DateTime.Parse(Console.ReadLine() ?? DateTime.Now.ToString("yyyy-MM-dd"));

            string password = null;

            if (roleId == 1 || roleId == 2)
            {
                password = GeneratePassword();
                Console.WriteLine($"Generated password for new admin member: {password}");
            }

            var member = new MemberDto
            {
                FullName = name,
                Email = email,
                PhoneNumber = phone,
                RoleId = roleId,
                JoinDate = joinDate,
                IsActive = true,
                PasswordHash = password ?? ""   
            };

            bool success = service.CreateMember(member);
            Console.WriteLine(success ? "Member created successfully!" : "Failed to create member.");
        }


        private static void UpdateMember(InterfaceSimsService service)
        {
            if (!AuthenticateAdmin(service))
                return;

            Console.Write("Member Id to update: ");
            int id = int.Parse(Console.ReadLine() ?? "0");

            var existing = service.GetMemberById(id);
            if (existing == null) { Console.WriteLine("Member not found."); return; }

            Console.Write($"Full Name ({existing.FullName}): ");
            string name = Console.ReadLine() ?? existing.FullName;

            Console.Write($"Email ({existing.Email}): ");
            string email = Console.ReadLine() ?? existing.Email;

            Console.Write($"Phone ({existing.PhoneNumber}): ");
            string phone = Console.ReadLine() ?? existing.PhoneNumber;

            Console.Write($"Role Id ({existing.RoleId}): ");
            int roleId = int.Parse(Console.ReadLine() ?? existing.RoleId.ToString());

            Console.Write($"Join Date ({existing.JoinDate:yyyy-MM-dd}): ");
            DateTime joinDate = DateTime.Parse(Console.ReadLine() ?? existing.JoinDate.ToString("yyyy-MM-dd"));

            existing.FullName = name;
            existing.Email = email;
            existing.PhoneNumber = phone;
            existing.RoleId = roleId;
            existing.JoinDate = joinDate;

            if (roleId == 1 || roleId == 2)
            {
                string newPassword = GeneratePassword();
                existing.PasswordHash = newPassword;
                Console.WriteLine($"New password assigned to this admin member: {newPassword}");
            }

            bool success = service.UpdateMember(existing);
            Console.WriteLine(success ? "Member updated successfully!" : "Failed to update member.");
        }



        private static void DeactivateMember(InterfaceSimsService service)
        {
            if (!AuthenticateAdmin(service))
                return;

            Console.Write("Member Id to deactivate: ");
            int id = int.Parse(Console.ReadLine() ?? "0");

            bool success = service.DeactivateMember(id);
            Console.WriteLine(success ? "Member deactivated." : "Failed to deactivate member.");
        }


        private static void ListRoles(InterfaceSimsService service)
        {
            var roles = service.GetRoles();
            Console.WriteLine("\nRoles:");
            foreach (var r in roles)
            {
                Console.WriteLine($"{r.RoleId}: {r.RoleName} - {r.Description}");
            }
        }

        private static void ListUpcomingEvents(InterfaceSimsService service)
        {
            var events = service.GetUpcomingEvents();
            Console.WriteLine("\nUpcoming Events:");
            foreach (var evt in events)
            {
                Console.WriteLine($"{evt.EventId}: {evt.Title} on {evt.EventDate:d} at {evt.Venue}, Budget: {evt.BudgetAllocated:C}");
            }
        }

        private static void CreateEventWithBudget(InterfaceSimsService service)
        {
            Console.Write("Title: "); string title = Console.ReadLine() ?? "";
            Console.Write("Description: "); string desc = Console.ReadLine() ?? "";
            Console.Write("Event Date (yyyy-MM-dd): "); DateTime date = DateTime.Parse(Console.ReadLine() ?? DateTime.Now.ToString());
            Console.Write("Venue: "); string venue = Console.ReadLine() ?? "";
            Console.Write("Organized By Member Id: "); int memberId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Budget Allocated: "); decimal budget = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Capacity: "); int capacity = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Budget Id: "); int budgetId = int.Parse(Console.ReadLine() ?? "0");

            var evt = new EventDto
            {
                Title = title,
                Description = desc,
                EventDate = date,
                Venue = venue,
                OrganizedByMemberId = memberId,
                BudgetAllocated = budget,
                Capacity = capacity
            };

            bool success = service.CreateEventWithBudget(evt, budgetId);
            Console.WriteLine(success ? "Event created successfully!" : "Failed to create event.");
        }

        // -------------------- Budgets & Expenses --------------------
        private static void AddExpense(InterfaceSimsService service)
        {
            Console.Write("Title: "); string title = Console.ReadLine() ?? "";
            Console.Write("Budget Id: "); int budgetId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Amount: "); decimal amount = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Expense Date (yyyy-MM-dd): "); DateTime date = DateTime.Parse(Console.ReadLine() ?? DateTime.Now.ToString());
            Console.Write("Created By Member Id: "); int memberId = int.Parse(Console.ReadLine() ?? "0");

            var expense = new ExpenseDto
            {
                Title = title,
                BudgetId = budgetId,
                Amount = amount,
                ExpenseDate = date,
                CreatedByMemberId = memberId
            };

            bool success = service.AddExpense(expense);
            Console.WriteLine(success ? "Expense added successfully!" : "Failed to add expense.");
        }

        private static void ShowEventFinancialSummary(InterfaceSimsService service)
        {
            var summaries = service.GetEventFinancialSummary();
            Console.WriteLine("\nEvent Financial Summary:");
            foreach (var s in summaries)
            {
                Console.WriteLine($"{s.EventTitle} ({s.EventDate:d}) - Allocated: {s.TotalAllocated:C}, Expenses: {s.TotalExpenses:C}, Remaining: {s.RemainingAmount:C}");
            }
        }

        private static void ShowMemberParticipationSummary(InterfaceSimsService service)
        {
            var summaries = service.GetMemberParticipationSummary();
            Console.WriteLine("\nMember Participation Summary:");
            foreach (var s in summaries)
            {
                Console.WriteLine($"{s.FullName} ({s.RoleName}) - Registered: {s.TotalRegistered}, Attended: {s.TotalAttended}, Cancelled: {s.TotalCancelled}, Attendance Rate: {s.AttendanceRate:P}");
            }
        }

        private static void ListBudgets(InterfaceSimsService service)
        {
            var budgets = service.GetBudgets();
            Console.WriteLine("\nBudgets:");
            foreach (var b in budgets)
            {
                Console.WriteLine($"{b.BudgetId}: {b.BudgetName}, Total: {b.TotalFunds:C}, Used: {b.FundsUsed:C}, Remaining: {b.RemainingFunds:C}");
            }
        }

        // -------------------- Announcements --------------------
        private static void CreateAnnouncement(InterfaceSimsService service)
        {
            Console.Write("Title: "); string title = Console.ReadLine() ?? "";
            Console.Write("Message: "); string msg = Console.ReadLine() ?? "";
            Console.Write("Event Id: "); int eventId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Created By Member Id: "); int memberId = int.Parse(Console.ReadLine() ?? "0");

            var ann = new AnnouncementDto
            {
                Title = title,
                Message = msg,
                EventId = eventId,
                CreatedByMemberId = memberId,
                CreatedAt = DateTime.Now
            };

            bool success = service.CreateAnnouncementAndNotify(ann);
            Console.WriteLine(success ? "Announcement created!" : "Failed to create announcement.");
        }

        private static void ListRecentAnnouncements(InterfaceSimsService service)
        {
            Console.Write("Max count: "); int max = int.Parse(Console.ReadLine() ?? "20");
            var anns = service.GetRecentAnnouncements(max);
            Console.WriteLine("\nRecent Announcements:");
            foreach (var a in anns)
            {
                Console.WriteLine($"{a.AnnouncementId}: {a.Title} ({a.CreatedAt:d}) - {a.Message}");
            }
        }

        // -------------------- Notifications --------------------
        private static void ListNotifications(InterfaceSimsService service)
        {
            Console.Write("Member Id: "); int memberId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Only unread? (y/n): "); bool onlyUnread = (Console.ReadLine() ?? "n").ToLower() == "y";

            var notifs = service.GetNotificationsForMember(memberId, onlyUnread);
            Console.WriteLine("\nNotifications:");
            foreach (var n in notifs)
            {
                Console.WriteLine($"{n.NotificationId}: {(n.IsRead ? "" : "[UNREAD] ")}{n.Message} ({n.CreatedAt:g})");
            }
        }

        private static void MarkNotificationAsRead(InterfaceSimsService service)
        {
            Console.Write("Notification Id: "); int id = int.Parse(Console.ReadLine() ?? "0");
            bool success = service.MarkNotificationAsRead(id);
            Console.WriteLine(success ? "Marked as read." : "Failed to mark as read.");
        }
    }
}

