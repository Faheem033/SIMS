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
            string mode = "";
            while (true)
            {
                Console.WriteLine("Choose backend mode:");
                Console.WriteLine("1. LINQ");
                Console.WriteLine("2. Stored Procedures");
                Console.Write("Enter choice (1/2): ");

                var modeChoice = Console.ReadLine();

                if (modeChoice == "1")
                {
                    mode = "LINQ";
                    break;
                }
                else if (modeChoice == "2")
                {
                    mode = "SP";
                    break;
                }
                else
                {
                    Console.WriteLine("\nInvalid choice! Please enter 1 or 2.\n");
                }
            }
            Console.WriteLine($"\n>> Starting SIMS in {mode} Mode...");
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
                Console.WriteLine("16. Register member for event");
                Console.WriteLine("17. Show Budget Utilization (Risk Report)");
                Console.WriteLine("18. Show Attendance Trends");
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
                    case "16": RegisterMember(service); break;
                    case "17": ShowBudgetUtilizationSummary(service); break;
                    case "18": ShowAttendanceTrend(service); break;
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
        private static void RegisterMember(InterfaceSimsService service)
        {
            int memberId = GetValidInt("Member Id: ");
            int eventId = GetValidInt("Event Id: ");

            bool success = service.RegisterMemberForEvent(memberId, eventId);
            if (success) 
            {
                Console.WriteLine("Success: Member registered for event.");
            }
            else 
            {
                Console.WriteLine("Failed: Member already registered or IDs are invalid.");
            }
        }
        private static void ShowBudgetUtilizationSummary(InterfaceSimsService service)
        {
            var summary = service.GetBudgetUtilizationSummary();
            Console.WriteLine("\n===== BUDGET UTILIZATION REPORT =====");
            Console.WriteLine($"{"ID",-5} | {"Budget Name",-20} | {"Total",-10} | {"Used",-10} | {"% Used",-8} | {"Risk",-10}");
            Console.WriteLine(new string('-', 80));

            foreach (var b in summary)
            {
                string riskIndicator = b.RiskLevel;
                if (b.RiskLevel == "Overspent" || b.RiskLevel == "High") riskIndicator = $"!! {b.RiskLevel} !!";

                Console.WriteLine(
                    $"{b.BudgetId,-5} | " +
                    $"{b.BudgetName,-20} | " +
                    $"{b.TotalFunds,-10:C0} | " + // C0 = Currency with 0 decimals
                    $"{b.FundsUsed,-10:C0} | " +
                    $"{b.UsagePercent,-8:F1}% | " + // F1 = 1 decimal place
                    $"{riskIndicator,-10}"
                );
            }
            Console.WriteLine(new string('-', 80));
        }

        private static void ShowAttendanceTrend(InterfaceSimsService service)
        {
            var trends = service.GetAttendanceTrend();
            Console.WriteLine("\n===== ATTENDANCE TREND ANALYSIS =====");
            Console.WriteLine("Grouping by Event, Year, and Month to show engagement over time.\n");

            Console.WriteLine($"{"Year-Month",-12} | {"Event ID",-8} | {"Event Title",-25} | {"Attendance",-10}");
            Console.WriteLine(new string('-', 65));

            foreach (var t in trends)
            {
                // Formatting Date as "2025-10"
                string period = $"{t.Year}-{t.Month:00}"; 

                Console.WriteLine(
                    $"{period,-12} | " +
                    $"{t.EventId,-8} | " +
                    $"{t.EventTitle,-25} | " +
                    $"{t.TotalAttendance,-10}"
                );
            }
            Console.WriteLine(new string('-', 65));
        }
        private static bool AuthenticateAdmin(InterfaceSimsService service)
        {
            // Replaced parsing with GetValidInt
            int id = GetValidInt("Enter your Member Id: ");

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

            // Replaced parsing with GetValidString
            string pass = GetValidString("Enter your Password: ");

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

            string name = GetValidString("Full Name: ");
            string email = GetValidString("Email: ");
            string phone = GetValidString("Phone: ");
            int roleId = GetValidInt("Role Id: ");
            DateTime joinDate = GetValidDate("Join Date (yyyy-MM-dd): ");

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

            int id = GetValidInt("Member Id to update: ");

            var existing = service.GetMemberById(id);
            if (existing == null) { Console.WriteLine("Member not found."); return; }

            Console.Write($"Full Name ({existing.FullName}): ");
            string nameInput = Console.ReadLine();
            string name = string.IsNullOrWhiteSpace(nameInput) ? existing.FullName : nameInput;

            Console.Write($"Email ({existing.Email}): ");
            string emailInput = Console.ReadLine();
            string email = string.IsNullOrWhiteSpace(emailInput) ? existing.Email : emailInput;

            Console.Write($"Phone ({existing.PhoneNumber}): ");
            string phoneInput = Console.ReadLine();
            string phone = string.IsNullOrWhiteSpace(phoneInput) ? existing.PhoneNumber : phoneInput;
            
            int roleId = GetValidIntOrKeep("Role Id", existing.RoleId);

            DateTime joinDate = GetValidDateOrKeep("Join Date", existing.JoinDate);

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

            int id = GetValidInt("Member Id to deactivate: ");

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

            string title = GetValidString("Title: ");
            string desc = GetValidString("Description: ");
            DateTime date = GetValidDate("Event Date (yyyy-MM-dd): ");
            string venue = GetValidString("Venue: ");
            int memberId = GetValidInt("Organized By Member Id: ");
            decimal budget = GetValidDecimal("Budget Allocated: ");
            int capacity = GetValidInt("Capacity: ");
            int budgetId = GetValidInt("Budget Id: ");

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
            string title = GetValidString("Title: ");
            int budgetId = GetValidInt("Budget Id: ");
            decimal amount = GetValidDecimal("Amount: ");
            DateTime date = GetValidDate("Expense Date (yyyy-MM-dd): ");
            int memberId = GetValidInt("Created By Member Id: ");

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
                Console.WriteLine($"{s.FullName} ({s.RoleName}) - Registered: {s.TotalRegistered}, Attended: {s.TotalAttended}, Cancelled: {s.TotalCancelled}, Attendance Rate: {s.AttendanceRate:F2}%");
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
            string title = GetValidString("Title: ");
            string msg = GetValidString("Message: ");
            int eventId = GetValidInt("Event Id: ");
            int memberId = GetValidInt("Created By Member Id: ");

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
            int max = GetValidInt("Max count: ");
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
            int memberId = GetValidInt("Member Id: ");
            bool onlyUnread = GetYesNo("Only unread?");

            var notifs = service.GetNotificationsForMember(memberId, onlyUnread);
            Console.WriteLine("\nNotifications:");
            foreach (var n in notifs)
            {
                Console.WriteLine($"{n.NotificationId}: {(n.IsRead ? "" : "[UNREAD] ")}{n.Message} ({n.CreatedAt:g})");
            }
        }

        private static void MarkNotificationAsRead(InterfaceSimsService service)
        {
            int id = GetValidInt("Notification Id: ");
            bool success = service.MarkNotificationAsRead(id);
            Console.WriteLine(success ? "Marked as read." : "Failed to mark as read.");
        }

        // -------------------- HELPER VALIDATORS --------------------
        private static bool GetYesNo(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (y/n): ");
                string input = (Console.ReadLine() ?? "").Trim().ToLower();

                if (input == "y" || input == "yes") return true;
                if (input == "n" || input == "no") return false;

                Console.WriteLine("Invalid input! Please enter 'y' for Yes or 'n' for No.");
            }
        }

        private static int GetValidInt(string prompt)
        {
            int value;
            Console.Write(prompt);
            // Added .Trim() to handle accidental spaces
            while (!int.TryParse((Console.ReadLine() ?? "").Trim(), out value))
            {
                Console.WriteLine("Invalid input! Please enter a valid number.");
                Console.Write(prompt);
            }
            return value;
        }

        private static decimal GetValidDecimal(string prompt, bool allowNegative = false)
        {
            decimal value;
            Console.Write(prompt);
            // Added .Trim()
            while (!decimal.TryParse((Console.ReadLine() ?? "").Trim(), out value) || (!allowNegative && value < 0))
            {
                if (value < 0) Console.WriteLine("Amount cannot be negative!");
                else Console.WriteLine("Invalid input! Please enter a valid amount (e.g., 50.00).");
                Console.Write(prompt);
            }
            return value;
        }

        private static DateTime GetValidDate(string prompt)
        {
            DateTime date;
            Console.Write(prompt);
            // Added .Trim()
            while (!DateTime.TryParse((Console.ReadLine() ?? "").Trim(), out date))
            {
                Console.WriteLine("Invalid Date! Please use format yyyy-MM-dd (e.g., 2025-12-31).");
                Console.Write(prompt);
            }
            return date;
        }

        private static string GetValidString(string prompt)
        {
            string input = "";
            while (string.IsNullOrWhiteSpace(input))
            {
                Console.Write(prompt);
                input = (Console.ReadLine() ?? "").Trim(); // Added Trim here too
                if (string.IsNullOrWhiteSpace(input)) Console.WriteLine("Input cannot be empty.");
            }
            return input;
        }

        // --- SPECIAL HELPERS FOR UPDATE (Safe + "Press Enter to Keep") ---

        private static int GetValidIntOrKeep(string prompt, int existingValue)
        {
            int newValue;
            while (true)
            {
                Console.Write($"{prompt} ({existingValue}): ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(input)) return existingValue;
                if (int.TryParse(input, out newValue)) return newValue;

                Console.WriteLine("Invalid number! Please enter a valid ID or press Enter to keep existing.");
            }
        }

        private static DateTime GetValidDateOrKeep(string prompt, DateTime existingValue)
        {
            DateTime newDate;
            while (true)
            {
                Console.Write($"{prompt} ({existingValue:yyyy-MM-dd}): ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(input)) return existingValue;
                if (DateTime.TryParse(input, out newDate)) return newDate;

                Console.WriteLine("Invalid Date! Format: yyyy-MM-dd (or press Enter to keep existing).");
            }
        }
    }
}