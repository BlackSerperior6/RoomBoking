using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace RoomBooking.Pages
{
    public class SeeAllRoomsModel : PageModel
    {
        public List<Room> Rooms { get; set; } = new List<Room>();

        public async Task<IActionResult> OnGetAsync()
        {
            string query = "SELECT * FROM \"Rooms\"";

            try
            {
                await using var connection = DatabaseConnectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(query, connection);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var room = new Room(reader.GetInt64(0), reader.GetInt64(4), reader.GetString(1),
                    reader.GetString(2), reader.GetDecimal(3));

                    Rooms.Add(room);
                }

                return Page();
            }
            catch (Exception ex) 
            {
                return RedirectToPage("/Profile", new {errorMessage = $"Ошибка при выполнении запроса:\n{ex}" }); 
            }
        }
    }
}
