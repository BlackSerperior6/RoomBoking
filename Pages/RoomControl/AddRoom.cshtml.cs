using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;

namespace RoomBooking.Pages.RoomControl
{
    [Authorize]
    public class AddRoomModel : PageModel
    {
        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public decimal PricePerHour { get; set; } = 1m;

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string query = $"INSERT INTO \"Rooms\" (\"RoomId\", \"Description\", \"Address\", " +
                       $"\"PricePerHour\", \"OwnerId\") VALUES (DEFAULT, @description, @address, @pricePerHour, @ownerId);";

            try
            {
                if (string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(Address))
                {
                    ErrorMessage = "Все поля должны быть заполнены!";
                    return Page();
                }

                await using var connection = DatabaseConnectionFactory.CreateConnection();
                await using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@description", NpgsqlDbType.Text, Description);
                command.Parameters.AddWithValue("@address", NpgsqlDbType.Text, Address);
                command.Parameters.AddWithValue("@pricePerHour", NpgsqlDbType.Numeric, PricePerHour);
                command.Parameters.AddWithValue("@ownerId", NpgsqlDbType.Bigint, User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex) 
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{ex}";
                return Page();
            }

            return RedirectToPage("/Profile", new {successMessage = "Комната успешно добавлена!" });
        }
    }
}
