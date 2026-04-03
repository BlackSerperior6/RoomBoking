using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using RoomBooking.Interfaces;
using System.Security.Claims;

namespace RoomBooking.Pages.BookingControls
{
    [Authorize]
    public class MakeBookingModel : PageModel
    {
        private IDatabaseConnectionFactory _connectionFactory;
        private IUserContextWrapper _userContext;

        public MakeBookingModel(IDatabaseConnectionFactory connectionFactory,
            IUserContextWrapper userContext)
        {
            _connectionFactory = connectionFactory;
            _userContext = userContext;
        }

        [BindProperty]
        public long RoomId { get; set; } = 1;

        [BindProperty]
        public DateTime StartDate {get; set;} = DateTime.Now;

        [BindProperty]
        public DateTime EndDate {get; set;} = DateTime.Now;

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string selectQuery = $"SELECT * FROM prod.\"Bookings\" WHERE \"RoomId\" = @roomId AND \"StartTime\" < @endTime AND \"EndTime\" > @startTime FOR UPDATE;";
            string insertQuery = $"INSERT INTO prod.\"Bookings\" (\"BookingId\", \"UserId\", \"RoomId\", \"StartTime\", \"EndTime\") VALUES (DEFAULT, @userID, @roomId, @startTime, @endTime);";

            try
            {
                if (StartDate >= EndDate)
                {
                    ErrorMessage = "Вы путешевстник во времени? (Дата конца должны быть больше даты начала брони)";
                    return Page();
                }

                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();

                await using var selectCommand = connection.CreateCommand(selectQuery);

                selectCommand.AddParameter("@roomId", NpgsqlDbType.Bigint, RoomId);
                selectCommand.AddParameter("@startTime", NpgsqlDbType.Timestamp, StartDate);
                selectCommand.AddParameter("@endTime", NpgsqlDbType.Timestamp, EndDate);

                await using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        ErrorMessage = $"Данная комната уже заблокирована на это время!";
                        return Page();
                    }
                }

                await using var insertCommand = connection.CreateCommand(insertQuery);

                insertCommand.AddParameter("@userId", NpgsqlDbType.Bigint, _userContext.GetCurrentUserId());

                insertCommand.AddParameter("@roomId", NpgsqlDbType.Bigint, RoomId);
                insertCommand.AddParameter("@startTime", NpgsqlDbType.Timestamp, StartDate);
                insertCommand.AddParameter("@endTime", NpgsqlDbType.Timestamp, EndDate);

                await insertCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                return RedirectToPage("/Profile", new { successMessage = "Бронь успешно создана!" });

            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                ErrorMessage = "Комнаты с таким id не существует!";
                return Page();
            }
            catch (Exception ex) 
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{ex}";
                return Page();
            } 

        }

    }

}