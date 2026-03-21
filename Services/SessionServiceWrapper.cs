namespace RoomBooking.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        private ISession Session => _httpContextAccessor.HttpContext?.Session 
            ?? throw new InvalidOperationException("Session is not available");

        public void SetString(string key, string value)
        {
            Session.SetString(key, value);
        }
        
        public string GetString(string key)
        {
            return Session.GetString(key);
        }
        
        public void Remove(string key)
        {
            Session.Remove(key);
        }
        
        public void Clear()
        {
            Session.Clear();
        }
    }
}