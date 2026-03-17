using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;

namespace RoomBooking.Pages.RoomControl
{
    public class RemoveRoomModel : PageModel
    {
        [BindProperty]
        public long RoomId { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string query = "DELETE FROM \"Rooms\" WHERE \"RoomId\" = @id";

            try
            {
                await using var connection = DatabaseConnectionFactory.CreateConnection();
                await using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", NpgsqlDbType.Text, RoomId);

                var affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    ErrorMessage = "Не существует комнаты с таким id";
                    return Page();
                }

                return RedirectToPage("/Profile", new {successMessage = "Комната была успешно удалена"});

            }
            catch (Exception ex) 
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{ex}";
                return Page();
            }

        }
    }
}