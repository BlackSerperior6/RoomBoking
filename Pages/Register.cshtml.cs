using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;

namespace RoomBooking.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string query = "INSERT INTO \"Users\" (\"UserId\", \"Login\", \"PasswordHash\") VALUES (DEFAULT, @login, @passwordHash)";

            try
            {
                if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Все поля должны быть заполнены!";
                    return Page();
                }

                await using var connection = DatabaseConnectionFactory.CreateConnection();
                await connection.OpenAsync();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

                await using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@login", NpgsqlDbType.Text, Login);
                command.Parameters.AddWithValue("@passwordHash", NpgsqlDbType.Text, hashedPassword);

                await command.ExecuteNonQueryAsync();
            }
            catch (NpgsqlException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                ErrorMessage = "Такой логин уже занят!";
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
