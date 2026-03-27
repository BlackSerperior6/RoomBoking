using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using RoomBooking.Interfaces;
using System.Security.Claims;

namespace RoomBooking.Pages.RoomControl
{
    public class RedactRoomModel : PageModel
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IUserContextWrapper _userContext;
        private readonly ISessionService _sessionService;

        public RedactRoomModel(
            IDatabaseConnectionFactory connectionFactory,
            IUserContextWrapper userContext,
            ISessionService sessionService)
            {
                _connectionFactory = connectionFactory;
                _userContext = userContext;
                _sessionService = sessionService;
            }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public decimal PricePerHour { get; set; } = 1m;

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(long roomId)
        {
            string query = "SELECT * FROM \"Rooms\" WHERE \"RoomId\" = @roomId AND \"OwnerId\" = @ownerId";

            try
            {
                var userId = _userContext.GetCurrentUserId();

                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand(query);

                command.AddParameter("@roomId", NpgsqlDbType.Bigint, roomId);
                command.AddParameter("@ownerId", NpgsqlDbType.Bigint, userId);

                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return RedirectToPage("/Profile", new
                    {
                        errorMessage = "Не существует комнаты с таким id под вашим контролем!"
                    });
                }

                Description = reader.GetString(1);
                Address = reader.GetString(2);
                PricePerHour = reader.GetDecimal(3);

                _sessionService.SetString("RoomId", roomId.ToString());
                _sessionService.SetString("EntryVersion", reader.GetInt64(5).ToString());

                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Profile", new
                {
                    errorMessage = $"Ошибка при выполнении запроса: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(Address))
            {
                ErrorMessage = "Все поля должны быть заполнены!";
                return Page();
            }

            // Get data from session using session service
            var roomIdString = _sessionService.GetString("RoomId");
            var entryVersionString = _sessionService.GetString("EntryVersion");

            if (!long.TryParse(roomIdString, out var roomId) ||
                !long.TryParse(entryVersionString, out var entryVersion))
            {
                ErrorMessage = "Не удалось получить данные из HTTP контекста!";
                return Page();
            }

            // Build query with optimistic concurrency
            var newVersion = entryVersion + 1;

            const string query = @"
                UPDATE ""Rooms"" 
                SET ""Description"" = @description, 
                    ""Address"" = @address, 
                    ""PricePerHour"" = @pricePerHour, 
                    ""version"" = @newVersion 
                WHERE ""RoomId"" = @roomId AND ""version"" = @currentVersion";

            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand(query);

                command.AddParameter("@description", NpgsqlDbType.Text, Description);
                command.AddParameter("@address", NpgsqlDbType.Text, Address);
                command.AddParameter("@pricePerHour", NpgsqlDbType.Numeric, PricePerHour);
                command.AddParameter("@roomId", NpgsqlDbType.Bigint, roomId);
                command.AddParameter("@newVersion", NpgsqlDbType.Bigint, newVersion);
                command.AddParameter("@currentVersion", NpgsqlDbType.Bigint, entryVersion);

                var updated = await command.ExecuteNonQueryAsync();

                if (updated == 0)
                {
                    return RedirectToPage("/Profile", new
                    {
                        errorMessage = "Запись о данной комнате была удалена или изменена. " +
                                       "Пожалуйста, начните процесс сначала!"
                    });
                }

                // Clear session data after successful update
                _sessionService.Remove("RoomId");
                _sessionService.Remove("EntryVersion");

                return RedirectToPage("/Profile", new
                {
                    successMessage = "Комната была успешно обновлена!"
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при выполнении запроса: {ex.Message}";
                return Page();
            }
        }
    }  
}