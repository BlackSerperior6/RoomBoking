using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using RoomBooking.Interfaces;
using System.Security.Claims;

namespace RoomBooking.Pages.BookingControls
{
    [Authorize]
    public class CancelBookingModel : PageModel
    {
        private IDatabaseConnectionFactory _connectionFactory;

        private IUserContextWrapper _userContext;

        public CancelBookingModel(IDatabaseConnectionFactory connectionFactory, IUserContextWrapper userContext)
        {
            _connectionFactory = connectionFactory;
            _userContext = userContext;
        }

        [BindProperty]
        public long BookingId { get; set; } = 1;

        public string ErrorMessage {get; set;}

        public async Task<IActionResult> OnPostAsync()
        {
            string userCheckQuery = "SELECT * FROM \"Bookings\" WHERE \"BookingId\" = @bookingId;";
            string cancelBookingQuery = "DELETE FROM \"Bookings\" WHERE \"BookingId\" = @bookingId;";

            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var checkUserCommand = connection.CreateCommand(userCheckQuery);

                checkUserCommand.AddParameter("@bookingId", NpgsqlDbType.Bigint, BookingId);

                await using var reader = await checkUserCommand.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    ErrorMessage = "Не существует брони с таким id!";
                    return Page();
                }

                var userId = _userContext.GetCurrentUserId();
                var bookerId = reader.GetInt64(1);

                await reader.CloseAsync();

                if (bookerId != userId)
                {
                    ErrorMessage = "Данная бронь не принадлежит вам!";
                    return Page();
                }

                await using var cancelBookingCommand = connection.CreateCommand(cancelBookingQuery);

                cancelBookingCommand.AddParameter("@bookingId", NpgsqlDbType.Bigint, BookingId);

                await cancelBookingCommand.ExecuteNonQueryAsync();

                return RedirectToPage("/Profile", new {successMessage = "Бронь была успешна удалена!" });
            }
            catch (Exception ex) 
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{ex}";
                return Page();
            }
        }
    }
}
