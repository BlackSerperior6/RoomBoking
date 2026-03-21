using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Dapper;

namespace RoomBooking.Pages
{
    public class AuthenticationModel : PageModel
    {
        [BindProperty]
        public string Login { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string query = $"SELECT \"UserId\", \"PasswordHash\" from \"Users\" WHERE \"Login\" = @login;";

            try
            {
                await using var connection = DatabaseConnectionFactory.CreateConnection();

                var user = await connection.QueryFirstOrDefaultAsync
                    <(long id, string passwordHash)>(query, new { login = Login });

                if (!string.IsNullOrWhiteSpace(user.passwordHash))
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(Password, user.passwordHash);

                    if (isValid)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, Login),
                            new Claim(ClaimTypes.NameIdentifier, user.id.ToString())
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

                        var returnUrl = Request.Query["returnUrl"].ToString();

                        if (!string.IsNullOrWhiteSpace(returnUrl))
                            return Redirect(returnUrl);
                        else
                            return RedirectToPage("/Profile");
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMessage = $"Ошибка при выполнении запроса:\n{e}!";
            }

            ErrorMessage = "Неверный логин или пароль!";
            return Page();
        }
    }
}
