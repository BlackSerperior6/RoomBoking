using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using RoomBooking.Interfaces;
using System.Security.Claims;

namespace RoomBooking.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private IDatabaseConnectionFactory _connectionFactory;
        private IUserContextWrapper _userContextWrapper;

        public ProfileModel(IDatabaseConnectionFactory connectionFactory, IUserContextWrapper userContextWrapper)
        {
            _connectionFactory = connectionFactory;
            _userContextWrapper = userContextWrapper;
        }

        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public decimal RoomIdRedact { get; set; }

        public string ErrorMessage {get; set;}

        public string SuccessMessage {get; set;}

        public List<Room> UsersRooms {get; set;} = new List<Room>();

        public List<RoomBookings> UserBookings {get; set;} = new List<RoomBookings>();

        public async Task<IActionResult> OnGetAsync(string successMessage = "", string errorMessage = "")
        {
            SuccessMessage = successMessage;
            ErrorMessage = errorMessage;

            string roomsQuery = "SELECT * FROM prod.rooms WHERE ownerid = @ownerId";
            string bookingQuery = "SELECT * FROM prod.bookings WHERE userid = @userId";

            try
            {
                Login = _userContextWrapper.GetCurrentUserLogin();

                long userId = _userContextWrapper.GetCurrentUserId();

                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var commandRoomsOfAUser = connection.CreateCommand(roomsQuery);

                commandRoomsOfAUser.AddParameter("@ownerId", NpgsqlDbType.Bigint, userId);

                await using var roomReader = await commandRoomsOfAUser.ExecuteReaderAsync();

                while (await roomReader.ReadAsync())
                {
                    var roomId = roomReader.GetInt64(0);

                    var room = new Room(roomId, userId, roomReader.GetString(1),
                    roomReader.GetString(2), roomReader.GetDecimal(3));

                    UsersRooms.Add(room);
                }

                await roomReader.CloseAsync();

                await using var commandBookingsOfAUser = connection.CreateCommand(bookingQuery);

                commandBookingsOfAUser.AddParameter("@userId", NpgsqlDbType.Bigint, userId);

                await using var usersBookingsReader = await commandBookingsOfAUser.ExecuteReaderAsync();

                while (await usersBookingsReader.ReadAsync())
                {
                    var booking = new RoomBookings(usersBookingsReader.GetInt64(0), usersBookingsReader.GetInt64(1), 
                    usersBookingsReader.GetInt64(2), usersBookingsReader.GetDateTime(3), usersBookingsReader.GetDateTime(4));

                    UserBookings.Add(booking);
                }

                await usersBookingsReader.CloseAsync();
            }
            catch (Exception e)
            {
                ErrorMessage = $"Ошибка при получения профиля:\n{e}";
                return Page();
            }

            return Page();
        }

        public IActionResult OnPostRedactRoom() 
        => RedirectToPage("/RoomControl/RedactRoom", new { roomId = RoomIdRedact });
    }

    public class Room
    {
        public long RoomId {get; set;}

        public long OwnerId {get; set;}

        public string Description {get; set;}
        public string Address {get; set;}
        public decimal PricePerHour {get; set;}

        public Room(long roomId, long ownerId, string description, string address, decimal pricePerHour)
        {
            RoomId = roomId;
            OwnerId = ownerId;
            Description = description;
            Address = address;
            PricePerHour = pricePerHour;
        }
    }

    public class RoomBookings
    {
        public long BookingId {get; set;}
        public long UserId { get; set; }
        public long RoomId {get; set;}
        public DateTime StartTime {get; set;}
        public DateTime EndTime {get; set;}

        public RoomBookings(long bookingId, long userId,long roomId, DateTime startTime, DateTime endTime)
        {
            BookingId = bookingId;
            UserId = userId;
            RoomId = roomId;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
