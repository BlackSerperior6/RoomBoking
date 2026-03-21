namespace RoomBooking.Interfaces
{
    public interface IDbCommandWrapper : IAsyncDisposable
    {
        void AddParameter(string name, NpgsqlDbType type, object value);
        Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default);
        Task<IDataReaderWrapper> ExecuteReaderAsync(CancellationToken cancellationToken = default);
    }
}