using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace RoomBooking.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public decimal RoomId { get; set; }

        public IActionResult OnGet()
        {
            Login = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            return Page();
        }

        public IActionResult OnPostRedactRoom() => RedirectToPage("/RoomControl/AddRoom", new { id = ViewBookId });
    }
}
