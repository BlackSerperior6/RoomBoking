using Npgsql;
using System.Data.Common;

namespace RoomBooking.Wrappers
{
    public class NpgsqlCommandWrapper : IDbCommandWrapper
    {
        private readonly NpgsqlCommand _command;
        
        public NpgsqlCommandWrapper(NpgsqlCommand command)
        {
            _command = command;
        }
        
        public void AddParameter(string name, NpgsqlDbType type, object value)
        {
            _command.Parameters.AddWithValue(name, type, value);
        }
        
        public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
        {
            return await _command.ExecuteNonQueryAsync(cancellationToken);
        }
        
        public async ValueTask DisposeAsync()
        {
            await _command.DisposeAsync();
        }
    }
}