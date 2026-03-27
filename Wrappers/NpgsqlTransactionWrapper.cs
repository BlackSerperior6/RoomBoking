using Npgsql;
using RoomBooking.Interfaces;

namespace RoomBooking.Wrappers
{
    public class NpgsqlTransactionWrapper : IDbTransactionWrapper
    {
        private NpgsqlTransaction _transaction;

        public NpgsqlTransactionWrapper(NpgsqlTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default) => await 
            _transaction.CommitAsync(cancellationToken);

        public async ValueTask DisposeAsync() => await _transaction.DisposeAsync();

        public async Task RollbackAsync(CancellationToken cancellationToken = default) => await 
            _transaction.RollbackAsync(cancellationToken);
    }
}
