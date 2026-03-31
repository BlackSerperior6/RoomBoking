using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using RoomBooking.Interfaces;
using RoomBooking.Wrappers;
using System.Security.Claims;

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
        public RemoveRoomModel(IDatabaseConnectionFactory dbConnectionFactory, IUserContextWrapper contextWrapper)
        {
            __connectionFactory = dbConnectionFactory;
            _contextWrapper = contextWrapper;
        }

        [BindProperty]
        public long RoomId { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string query = "DELETE FROM \"Rooms\" WHERE \"RoomId\" = @id AND \"OwnerId\" = @ownerId";

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