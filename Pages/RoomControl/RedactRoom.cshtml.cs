using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;

namespace RoomBooking.Pages.RoomControl
{
    [BindProperty]
    public string Description { get; set; }

    [BindProperty]
    public string Address { get; set; }

    [BindProperty]
    public decimal PricePerHour { get; set; } = 1m;

    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(long roomId)
    {
        string query = "SELECT * FROM \"Rooms\" WHERE \"RoomId\" = @roomId AND \"OwnerId\" = @ownerId";

        try
        {
            await using var connection = DatabaseConnectionFactory.CreateConnection();
            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@roomId", NpgsqlDbType.Bigint, roomId);
            command.Parameters.AddWithValue("@ownerId", NpgsqlDbType.Bigint, User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                await reader.CloseAsync();
                return RedirectToPage("/Profile", new { errorMessage = "Не существует комнаты с таким id пол вашим контролем!"});
            }

            Description = reader.GetString(1);
            Address = reader.GetString(2);
            PricePerHour = reader.GetDecimal(3);

            HttpContext.Session.SetString("RoomId", roomId.ToString());
            HttpContext.Session.SetString("EntryVersion", reader.GetInt64(5).ToString());

        }
        catch (Exception ex) 
        {
            return RedirectToPage("/Profile", new { errorMessage = $"Ошибка при выполнении запроса: {ex}"});
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Description) || 
            string.IsNullOrWhiteSpace(Address))
        {
            ErrorMessage = "Все поля должны быть заполнены!";
            return Page();
        }

        var roomIdString = HttpContext.Session.GetString("RoomId");
        var entryVersionString = HttpContext.Session.GetString("EntryVersion");

        if (!long.TryParse(roomIdString, out var roomId) || 
            !long.TryParse(entryVersionString, out var entryVersion))
        {
            ErrorMessage = "Не удалось получить данные из HTTP контекста!";
            return Page();
        }

        string query = $"UPDATE \"Rooms\" Set \"Description\" = @description, \"Address\" = @address, \"PricePerHour\" = @pricePerHour, \"version\" = {entryVersion + 1} WHERE \"RoomId\" = @roomId AND \"version\" = '{entryVersion}'";

        using var connection = DatabaseConnectionFactory.CreateConnection();
        using var command = new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue("@description", NpgsqlDbType.Text, Description);
        command.Parameters.AddWithValue("@address", NpgsqlDbType.Text, Address);
        command.Parameters.AddWithValue("@pricePerHour", NpgsqlDbType.Numeric, PricePerHour);
        command.Parameters.AddWithValue("@roomId", NpgsqlDbType.Bigint, roomId);

        var updated await command.ExecuteNonQueryAsync();

        if (updated == 0)
        {
            return RedirectToPage("/Profile", new {errorMessage = "Запись о данной комнате была удалена или изменена. Пожалуйста, начните процесс сначала!"});
        }

        return RedirectToPage("/Profile", new {successMessage = "Комната была успешно обновлена!"});
    }
 }