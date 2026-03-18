using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;

namespace RoomBooking.Pages.BookingControls
{
    [Authorize]
    public class MakeBookingModel : PageModel
    {
        [BindProperty]
        public long RoomId {get; set;}

        [BindProperty]
        public DateTime StartDate {get; set;} = DateTime.Now;

        [BindProperty]
        public DateTime EndDate {get; set;} = DateTime.Now;

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string selectQuery = $"SELECT * FROM \"Bookings\" WHERE \"RoomId\" = @roomId AND \"StartTime\" <= @startTime AND \"EndTime\" > @endTime FOR UPDATE;";
            string insertQuery = $"INSERT INTO \"Bookings\" (\"BookingId\", \"UserId\", \"RoomId\", \"StartTime\", \"EndTime\") VALUES (DEFAULT, @userID, @roomId, @startTime, @endTime);";

            try
            {
                await using var connection = DatabaseConnectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();

                await using var selectCommand = new NpgsqlCommand(selectQuery, connection);

                selectCommand.Parameters.AddWithValue("@roomId", NpgsqlDbType.Bigint, RoomId);
                selectCommand.Parameters.AddWithValue("@startTime", NpgsqlDbType.Date, StartDate);
                selectCommand.Parameters.AddWithValue("@endTime", NpgsqlDbType.Date, EndDate);

                await using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        ErrorMessage = $"Данная комната уже заблокирована на это время!";
                        return Page();
                    }
                }

                await using var insertCommand = new NpgsqlCommand(insertQuery, connection);

                insertCommand.Parameters.AddWithValue("@userId", NpgsqlDbType.Bigint, long.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value));
                insertCommand.Parameters.AddWithValue("@roomId", NpgsqlDbType.Bigint, RoomId);
                insertCommand.Parameters.AddWithValue("@startTime", NpgsqlDbType.Date, StartDate);
                insertCommand.Parameters.AddWithValue("@endTime", NpgsqlDbType.Date, EndDate);

                await insertCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                return RedirectToPage("/Profile", new { successMessage = "Бронь успешно создана!" });

            }
            catch (Exception ex) 
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{ex}";
                return Page();
            } 

        }

    }

}