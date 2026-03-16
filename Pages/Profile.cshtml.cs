using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace RoomBooking.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public decimal RoomId { get; set; }

        public string ErrorMessage {get; set;}

        public string SuccessMessage {get; set;}

        public List<Room> UsersRooms {get; set;} = new List<Room>();

        public List<RoomBookings> UserBookings {get; set;} = new List<RoomBookings>();

        public async Task<IActionResult> OnGetAsync(string successMessage = "", string errorMessage = "")
        {
            SuccessMessage = successMessage;
            ErrorMessage = errorMessage;

            Login = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            long userId = long.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            string roomsQuery = "SELECT * FROM \"Rooms\" WHERE \"OwnerId\" = @ownerId";

            try
            {
                await using var connection = DatabaseConnectionFactory.CreateConnection();
                await using (var command = new NpgsqlCommand(roomsQuery, connection))
                {
                    command.Parameters.AddWithValue("@ownerId", NpgsqlDbType.Bigint, userId);

                    await using var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var room = new Room(reader.GetInt64(0), reader.GetString(1), 
                        reader.GetString(2), reader.GetDecimal(3));

                        UsersRooms.Add(room);
                    }
                }

                string bookingQuery = "SELECT * FROM \"Bookings\" WHERE \"UserId\" = @userId";
                await using var command = new NpgsqlCommand(bookingQuery, connection);

                command.Parameters.AddWithValue(@userId, NpgsqlDbType.Bigint, userId);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var booking = new RoomBookings(reader.GetInt64(0), reader.GetInt64(2), 
                    reader.GetDateTime(3), reader.GetDateTime(4));

                    UserBookings.Add(booking);
                }

                return Page();

            }
            catch (Exception e)
            {
                ErrorMessage = $"Ошибка при получения профиля:\n{e}";
                return Page();

            }
            return Page();
        }

        public IActionResult OnPostRedactRoom() 
        => RedirectToPage("/RoomControl/AddRoom", new { id = ViewBookId });
    }

    public class Room
    {
        public long RoomId {get; set;}
        public string Description {get; set;}
        public string Address {get; set;}
        public decimal PricePerHour {get; set;}

        public Room(long roomId, string description, string address, decimal pricePerHour)
        {
            RoomId = roomId;
            Description = description;
            Address = address;
            PricePerHour = pricePerHour;
        }
    }

    public class RoomBookings
    {
        public long BookingId {get; set;}
        public long RoomId {get; set;}
        public DateTime StartTime {get; set;}
        public DateTime EndTime {get; set;}

        public RoomBookings(long bookingId, long roomId, DateTime startTime, DateTime endTime)
        {
            BookingId = bookingId;
            RoomId = roomId;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
