namespace RoomBooking.Interfaces
{
    public interface IDbConnectionWrapper : IAsyncDisposable
    {
        Task OpenAsync(CancellationToken cancellationToken = default);
        IDbCommandWrapper CreateCommand(string sql);
    }
}