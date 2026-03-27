using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;
using RoomBooking.Interfaces;
using RoomBooking.Wrappers;

namespace RoomBooking.Pages.RoomControl
{
    [Authorize]
    public class AddRoomModel : PageModel
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IUserContextWrapper _userContext;

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public decimal PricePerHour { get; set; } = 1m;

        public string ErrorMessage { get; set; }

        public AddRoomModel(
            IDatabaseConnectionFactory connectionFactory,
            IUserContextWrapper userContext)
        {
            _connectionFactory = connectionFactory;
            _userContext = userContext;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string query = $"INSERT INTO \"Rooms\" (\"RoomId\", \"Description\", \"Address\", " +
                       $"\"PricePerHour\", \"OwnerId\", \"version\") VALUES (DEFAULT, @description, @address, @pricePerHour, @ownerId, DEFAULT);";

            try
            {
                if (string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(Address))
                {
                    ErrorMessage = "Все поля должны быть заполнены!";
                    return Page();
                }

                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand(query);

                command.AddParameter("@description", NpgsqlDbType.Text, Description);
                command.AddParameter("@address", NpgsqlDbType.Text, Address);
                command.AddParameter("@pricePerHour", NpgsqlDbType.Numeric, PricePerHour);
                command.AddParameter("@ownerId", NpgsqlDbType.Bigint, _userContext.GetCurrentUserId());
                
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex) 
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{ex}";
                return Page();
            }

            return RedirectToPage("/Profile", new {successMessage = "Комната была успешно добавлена!" });
        }
    }
}
