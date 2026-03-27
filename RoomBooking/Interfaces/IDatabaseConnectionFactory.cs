namespace RoomBooking.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnectionWrapper CreateConnection();
    } 
}