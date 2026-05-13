using System.Data.SqlClient;
using System.Data;

namespace EnBiletBackend.Connection
{
    public class DapperContext
    {
        private readonly string _connectionString;
        public DapperContext()
        {

            _connectionString = "Data Source=Marty;Initial Catalog=cocukakli;Integrated Security=True;Pooling=false;";
        }
        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
