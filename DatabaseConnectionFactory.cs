using Npgsql;

namespace RoomBooking
{
    public static class DatabaseConnectionFactory
    {
        private static string _connectionString;

        public static void Init(IConfiguration configuration)
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

        public static NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    }
}
