using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NpgsqlTypes;
using RoomBooking.Interfaces;

namespace RoomBooking.Pages.RoomControl
{
    /// <summary>
    /// Page model for removing a room from the system.
    /// </summary>
    public class RemoveRoomModel : PageModel
    {
        private IDatabaseConnectionFactory __connectionFactory;

        private IUserContextWrapper _contextWrapper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbConnectionFactory">The connection factory to use for database connections.</param>
        /// <param name="contextWrapper">The user context wrapper to use in the operations.</param>
        public RemoveRoomModel(IDatabaseConnectionFactory dbConnectionFactory, IUserContextWrapper contextWrapper)
        {
            __connectionFactory = dbConnectionFactory;
            _contextWrapper = contextWrapper;
        }

        /// <summary>
        /// Id of the room to be removed.
        /// </summary>
        [BindProperty]
        public long RoomId { get; set; }

        /// <summary>
        /// Error message to display in case of an error occuring.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Handles the POST request to remove a room.
        /// </summary>
        /// <returns>
        /// Redirects to the profile page with success message on success,
        /// or returns the current page with error message on validation failure or exception.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            string query = "DELETE FROM prod.\"Rooms\" WHERE \"RoomId\" = @id AND \"OwnerId\" = @ownerId";

            try
            {
                await using var connection = __connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand(query);

                command.AddParameter("@id", NpgsqlDbType.Bigint, RoomId);
                command.AddParameter("@ownerId", NpgsqlDbType.Bigint, _contextWrapper.GetCurrentUserId());

                var affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    ErrorMessage = "Не существует комнаты с таким id принадлежащей вам";
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