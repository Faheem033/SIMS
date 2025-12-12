// using SIMS.BLL.LinqImpl;
// using SIMS.BLL.SpImpl;

// namespace SIMS.BLL.Factory
// {
//     public static class SimsServiceFactory
//     {
//         public static InterfaceSimsService Create(string mode)
//         {
//             mode = mode?.ToUpperInvariant() ?? "LINQ";

//             return mode == "SP"
//                 ? new SpSimsService()
//                 : new LinqSimsService();
//         }
//     }
// }

using SIMS.BLL.LinqImpl;
using SIMS.BLL.SpImpl;
using SIMS.Data.Entities; // Import your DbContext namespace

namespace SIMS.BLL.Factory
{
    public static class SimsServiceFactory
    {
        public static InterfaceSimsService Create(string mode)
        {
            mode = mode?.ToUpperInvariant() ?? "LINQ";

            if (mode == "SP")
            {
                return new SpSimsService();
            }
            else
            {
                // Create the DbContext and pass it to the LINQ service
                var context = new EventManagementDbContext();
                return new LinqSimsService(context);
            }
        }
    }
}

