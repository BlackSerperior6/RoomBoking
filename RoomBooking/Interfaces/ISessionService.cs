namespace RoomBooking.Interfaces
{
    public interface ISessionService
    {
        void SetString(string key, string value);
        string GetString(string key);
        void Remove(string key);
        void Clear();
    }
}