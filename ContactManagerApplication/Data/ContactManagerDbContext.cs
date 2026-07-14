using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ContactManagerApplication.Data
{
    public class ContactManagerDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ContactManagerDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("ContactManagerDbConnectionString");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
