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
    /// <summary>
    /// Page model for adding a new room to the system.
    /// This page handles the creation of room listings by authenticated users.
    /// </summary>
    /// <remarks>
    /// Only authorized users can access this page. The room will be associated 
    /// with the currently authenticated user as the owner.
    /// </remarks>
    [Authorize]
    public class AddRoomModel : PageModel
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IUserContextWrapper _userContext;

        /// <summary>
        /// Gets or sets the room description.
        /// </summary>
        [BindProperty]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the room address.
        /// </summary>
        [BindProperty]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the room price per hour.
        /// </summary>
        [BindProperty]
        public decimal PricePerHour { get; set; } = 1m;

        /// <summary>
        /// Gets or sets the error message of the POST request.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AddRoomModel(
            IDatabaseConnectionFactory connectionFactory,
            IUserContextWrapper userContext)
        {
            _connectionFactory = connectionFactory;
            _userContext = userContext;
        }

        /// <summary>
        /// Handles the POST request to create a new room.
        /// </summary>
        /// <returns>
        /// Redirects to the profile page with success message on success,
        /// or returns the current page with error message on validation failure or exception.
        /// </returns>
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
