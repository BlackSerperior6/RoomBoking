using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoomBooking.Interfaces;

namespace RoomBooking.Pages
{
    public class LogOutModel : PageModel
    {
        private IUserContextWrapper _userContext;

        public LogOutModel(IUserContextWrapper userContex)
        {
            _userContext = userContex;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _userContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
    }
}
