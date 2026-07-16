using System.Data;
using System.Data.Common;
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
            _connectionString = configuration.GetConnectionString("ContactManagerDbConnectionString")
                ?? throw new InvalidOperationException("Connection string 'ContactManagerDbConnectionString' is not configured.");
        }

        public DbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
