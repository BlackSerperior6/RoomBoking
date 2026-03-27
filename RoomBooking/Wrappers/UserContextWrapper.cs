using Microsoft.AspNetCore.Authentication;
using RoomBooking.Interfaces;
using System.Security.Claims;

namespace RoomBooking.Wrappers
{
    public class UserContext : IUserContextWrapper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserLogin()
        {
            var userLoginClaims =
            _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return string.IsNullOrWhiteSpace(userLoginClaims) ? "" : userLoginClaims;

        }
        
        public long GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            return userIdClaim != null ? long.Parse(userIdClaim) : 0;
        }

        public async Task SignInAsync(string scheme, ClaimsPrincipal principal)
        {
            await _httpContextAccessor.HttpContext?.SignInAsync(scheme, principal);
        }

        public async Task SignOutAsync(string scheme)
        {
            await _httpContextAccessor.HttpContext?.SignOutAsync(scheme);
        }
    }
}