using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace EnBiletBackend.Connection
{
    public class DapperContext
    {
        private readonly string _connectionString;



        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
