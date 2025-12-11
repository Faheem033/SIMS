using Microsoft.Data.SqlClient;

namespace SIMS.Data
{
    public static class SqlConnectionFactory
    {
        public static SqlConnection Create()
        {
            return new SqlConnection(DbConfig.ConnectionString);
        }
    }
}
