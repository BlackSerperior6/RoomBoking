using Npgsql;
using RoomBooking.Interfaces;

namespace RoomBooking.Wrappers
{
    public class NpgsqlDataReaderWrapper : IDataReaderWrapper
    {
        private NpgsqlDataReader _reader;

        public NpgsqlDataReaderWrapper(NpgsqlDataReader reader) 
        {
            _reader = reader;
        }

        public async ValueTask DisposeAsync() => await _reader.DisposeAsync();

        public decimal GetDecimal(int ordinal) => _reader.GetDecimal(ordinal);

        public T GetFieldValue<T>(int ordinal) => _reader.GetFieldValue<T>(ordinal);

        public long GetInt64(int ordinal) => _reader.GetInt64(ordinal);

        public string GetString(int ordinal) => _reader.GetString(ordinal);

        public DateTime GetDateTime(int ordinal) => _reader.GetDateTime(ordinal);

        public bool IsDBNull(int ordinal) => _reader.IsDBNull(ordinal);

        public async Task<bool> ReadAsync(CancellationToken cancellationToken = default) => 
            await _reader.ReadAsync(cancellationToken);

        public async Task CloseAsync(CancellationToken cancellationToken = default) => await _reader.CloseAsync();

        
    }
}
