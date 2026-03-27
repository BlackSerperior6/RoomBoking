using System.Security.Claims;

namespace RoomBooking.Interfaces
{
    public interface IUserContextWrapper
    {
        string GetCurrentUserLogin();
        long GetCurrentUserId();

        Task SignInAsync(string scheme, ClaimsPrincipal principal);

        Task SignOutAsync(string scheme);
    }
}