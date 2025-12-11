using SIMS.BLL.LinqImpl;
using SIMS.BLL.SpImpl;

namespace SIMS.BLL.Factory
{
    public static class SimsServiceFactory
    {
        public static InterfaceSimsService Create(string mode)
        {
            mode = mode?.ToUpperInvariant() ?? "LINQ";

            return mode == "SP"
                ? new SpSimsService()
                : new LinqSimsService();
        }
    }
}
