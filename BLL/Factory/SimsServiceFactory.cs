using SIMS.BLL.LinqImpl;
using SIMS.BLL.SpImpl;
using SIMS.Data.Entities; 

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
                var context = new EventManagementDbContext();
                return new LinqSimsService(context);
            }
        }
    }
}

