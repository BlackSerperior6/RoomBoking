namespace RoomBooking.Interfaces
{
    public interface IDbConnectionWrapper : IAsyncDisposable
    {
        Task OpenAsync(CancellationToken cancellationToken = default);

        Task<IDbTransactionWrapper> BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task<T> QueryFirstOrDefaultAsync<T>( string sql, object parameters = null);


        IDbCommandWrapper CreateCommand(string sql);
    }
}