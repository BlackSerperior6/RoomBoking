namespace RoomBooking.Interfaces
{
    public interface IDataReaderWrapper : IAsyncDisposable
    {
        Task<bool> ReadAsync(CancellationToken cancellationToken = default);
        string GetString(int ordinal);
        decimal GetDecimal(int ordinal);
        long GetInt64(int ordinal);
        T GetFieldValue<T>(int ordinal);
        bool IsDBNull(int ordinal);
    }

}