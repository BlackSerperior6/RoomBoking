namespace RoomBooking.Wrappers
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserLogin()
        {
            var userLoginClaims = 
            User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

            return string.IsNullOrWhiteSpace(userLoginClaims) ? "" : userLoginClaims;

        }
        
        public long GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            return userIdClaim != null ? long.Parse(userIdClaim) : 0;
        }
    }
}