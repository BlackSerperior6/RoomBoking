using Npgsql;
using RoomBooking.Interfaces;
using RoomBooking.Wrappers;

namespace RoomBooking
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private string _connectionString;

        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            var baseConnectionString = configuration.GetConnectionString("DefaultConnection");

            var dbPassword = Environment.GetEnvironmentVariable("DP_LIB_PASSWORD");

            if (string.IsNullOrEmpty(dbPassword))
            {
                throw new InvalidOperationException(
                    "DP_LIB_PASSWORD environment variable is not set. " +
                    "Please set it before running the application."
                );
            }

            _connectionString = $"{baseConnectionString};Password={dbPassword};";
        }

        public IDbConnectionWrapper CreateConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            return new NpgsqlConnectionWrapper(connection);
        }
    }
}
