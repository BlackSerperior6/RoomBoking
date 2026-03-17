using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;

namespace RoomBooking.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public decimal RoomIdAdd { get; set; }

        [BindProperty]
        public decimal RoomIdDelete { get; set; }

        [BindProperty]
        public decimal RoomIdRedact { get; set; }

        public string ErrorMessage {get; set;}

        public string SuccessMessage {get; set;}

        public List<Room> UsersRooms {get; set;} = new List<Room>();

        public List<RoomBookings> UserBookings {get; set;} = new List<RoomBookings>();

        public List<RoomBookings> BookingsOfAUsersRoom {get; set;} = new List<RoomBookings>();

        public async Task<IActionResult> OnGetAsync(string successMessage = "", string errorMessage = "")
        {
            SuccessMessage = successMessage;
            ErrorMessage = errorMessage;

            string roomsQuery = "SELECT * FROM \"Rooms\" WHERE \"OwnerId\" = @ownerId";
            string bookingQuery = "SELECT * FROM \"Bookings\" WHERE \"UserId\" = @userId";
            string bookingForARoomQuery = "SELECT * FROM \"Bookings\" WHERE \"RoomId\" in (SELECT \"RoomId\" FROM \"Rooms\" WHERE " +
                "\"OwnerId\" = @ownerId)";

            try
            {
                Login = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

                long userId = long.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

                await using var connection = DatabaseConnectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var commandRoomsOfAUser = new NpgsqlCommand(roomsQuery, connection);

                commandRoomsOfAUser.Parameters.AddWithValue("@ownerId", NpgsqlDbType.Bigint, userId);

                await using var roomReader = await commandRoomsOfAUser.ExecuteReaderAsync();

                while (await roomReader.ReadAsync())
                {
                    var roomId = roomReader.GetInt64(0);

                    var room = new Room(roomId, roomReader.GetString(1),
                    roomReader.GetString(2), roomReader.GetDecimal(3));

                    UsersRooms.Add(room);
                }

                await roomReader.CloseAsync();

                await using var commandBookingsOfAUser = new NpgsqlCommand(bookingQuery, connection);

                commandBookingsOfAUser.Parameters.AddWithValue("@userId", NpgsqlDbType.Bigint, userId);

                await using var usersBookingsReader = await commandBookingsOfAUser.ExecuteReaderAsync();

                while (await usersBookingsReader.ReadAsync())
                {
                    var booking = new RoomBookings(usersBookingsReader.GetInt64(0), usersBookingsReader.GetInt64(2), 
                    usersBookingsReader.GetDateTime(3), usersBookingsReader.GetDateTime(4));

                    UserBookings.Add(booking);
                }

                await usersBookingsReader.CloseAsync();

                await using var commandBookingsOfAUsersRoom = new NpgsqlCommand(bookingForARoomQuery, connection);

                await using var bookingsOfAUsersRoomReader = await commandBookingsOfAUser.ExecuteReaderAsync();

                while (await bookingsOfAUsersRoomReader.ReadAsync())
                {
                    var booking = new RoomBookings(bookingsOfAUsersRoomReader.GetInt64(0), bookingsOfAUsersRoomReader.GetInt64(2),
                    bookingsOfAUsersRoomReader.GetDateTime(3), bookingsOfAUsersRoomReader.GetDateTime(4));

                    BookingsOfAUsersRoom.Add(booking);
                }

                await bookingsOfAUsersRoomReader.CloseAsync();
            }
            catch (Exception e)
            {
                ErrorMessage = $"Ошибка при получения профиля:\n{e}";
                return Page();
            }

            return Page();
        }

        public IActionResult OnPostAddRoom() 
            => RedirectToPage("/RoomControl/AddRoom", new { id = RoomIdAdd });

        public IActionResult OnPostDeleteRoom() 
            => RedirectToPage("/RoomControl/DeleteRoom", new { id = RoomIdDelete });

        public IActionResult OnPostRedactRoom() 
        => RedirectToPage("/RoomControl/RedactRoom", new { id = RoomIdRedact });
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
