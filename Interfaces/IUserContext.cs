namespace RoomBooking.Interfaces
{
    public interface IUserContext
    {
        string GetCurrentUserLogin();
        long GetCurrentUserId();
    }
}