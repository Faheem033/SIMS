using System.Data;
using System.Data.SqlClient;

namespace SIMS.Data
{
    public static class SqlConnectionFactory
    {
        public static IDbConnection Create()
        {
            return new SqlConnection(DbConfig.ConnectionString);
        }
    }
}
