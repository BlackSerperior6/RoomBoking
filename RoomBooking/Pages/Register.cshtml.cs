using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using RoomBooking.Interfaces;

namespace RoomBooking.Pages
{
    public class RegisterModel : PageModel
    {
        private IDatabaseConnectionFactory _connectionFactory;

        public RegisterModel(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string query = "INSERT INTO prod.users (userId, login, passwordhash) VALUES (DEFAULT, @login, @passwordHash)";

            try
            {
                if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Все поля должы быть заполнены!";
                    return Page();
                }

                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

                await using var command = connection.CreateCommand(query);

                command.AddParameter("@login", NpgsqlDbType.Text, Login);
                command.AddParameter("@passwordHash", NpgsqlDbType.Text, hashedPassword);

                await command.ExecuteNonQueryAsync();
            }
            catch (NpgsqlException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                ErrorMessage = "Логин должен быть уникальным!";
                return Page();
            }
            catch (Exception ex) 
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{ex}";
                return Page();
            }

            SuccessMessage = "Регистрация прошла успешно!";
            return Page();
        }
    }
}
