using Dapper;
using Npgsql;
using RoomBooking.Interfaces;

namespace RoomBooking.Wrappers
{
    public class NpgsqlConnectionWrapper : IDbConnectionWrapper
    {
        private readonly NpgsqlConnection _connection;
        
        public NpgsqlConnectionWrapper(NpgsqlConnection connection)
        {
            _connection = connection;
        }
        
        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            await _connection.OpenAsync(cancellationToken);
        }
        
        public IDbCommandWrapper CreateCommand(string sql)
        {
            var command = new NpgsqlCommand(sql, _connection);
            return new NpgsqlCommandWrapper(command);
        }
        
        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
        }

        public async Task<IDbTransactionWrapper> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await _connection.BeginTransactionAsync(cancellationToken);
            return new NpgsqlTransactionWrapper(transaction);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null)
        {
            return await _connection.QueryFirstOrDefaultAsync<T>(
                sql,
                parameters);
        }
    } 
}