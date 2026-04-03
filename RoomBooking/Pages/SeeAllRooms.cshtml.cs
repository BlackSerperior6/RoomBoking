using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using RoomBooking.Interfaces;

namespace RoomBooking.Pages
{
    public class SeeAllRoomsModel : PageModel
    {
        private IDatabaseConnectionFactory _databaseConnectionFactory;

        public List<Room> Rooms { get; set; } = new List<Room>();

        public SeeAllRoomsModel(IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string query = "SELECT * FROM prod.rooms";

            try
            {
                await using var connection = _databaseConnectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand(query);

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
