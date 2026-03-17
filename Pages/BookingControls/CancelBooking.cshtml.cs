using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;

namespace RoomBooking.Pages.BookingControls
{
    [Authorize]
    public class CancelBookingModel : PageModel
    {
        [BindProperty]
        public long BookingId {get; set;}

        public async Task<IActionResult> OnGetAsync()
        {
            var checkQuery = "SELECT * FROM \"Bookings\" WHERE \"BookingId\" = @bookingId AND \"UserId\" = @userId"

        }
    }
}