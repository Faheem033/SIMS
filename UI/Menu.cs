using System;
using SIMS.BLL;
using SIMS.BLL.Factory;

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
                Console.WriteLine("0. Exit");
                Console.Write("Select option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.WriteLine("TODO: list upcoming events");
                        break;
                    case "2":
                        Console.WriteLine("TODO: prompt for event fields + budgetId, then call CreateEventWithBudget");
                        break;
                    case "3":
                        Console.WriteLine("TODO: prompt for expense fields, then call AddExpense");
                        break;
                    case "4":
                        Console.WriteLine("TODO: call GetEventFinancialSummary and print results");
                        break;
                    case "5":
                        Console.WriteLine("TODO: call GetMemberParticipationSummary and print");
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
            }
        }
    }
}
