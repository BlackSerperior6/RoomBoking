using Microsoft.Playwright;

namespace RoomBooking.UITests;

public class AddRoomTest : IAsyncLifetime
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private CustomWebApplicationFactory _factory;
    private string _baseUrl;

    public async Task DisposeAsync()
    {
        _playwright?.Dispose();

        await _browser?.CloseAsync();
        await (ValueTask)_factory?.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        _baseUrl = _factory.BaseUrl;
        
        await WaitForServerReady();
        
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox" }
        });
    }

    private async Task WaitForServerReady()
    {
        using var client = new HttpClient();
        var maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await client.GetAsync($"{_baseUrl}");

                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                await Task.Delay(500);
            }
        }

        throw new Exception("Server failed to start");
    }

    [Fact]
    public async Task AddRoomValidTest()
    {
        _factory.SetupMoq();
        var page = await _browser.NewPageAsync();
        
        await page.GotoAsync($"{_baseUrl}/RoomControl/AddRoom");
        
        await page.FillAsync("#Description", "Test Room");
        await page.FillAsync("#Address", "Test Address");
        await page.FillAsync("#PricePerHour", "50");
        
        await page.ClickAsync("button[type=submit]");
        
        await page.WaitForURLAsync(url => url.Contains("/Profile"));
        await page.WaitForSelectorAsync("#SuccessMessage");

        var successMessage = await page.TextContentAsync("#SuccessMessage");
        Assert.Contains("Комната была успешно добавлена!", successMessage);
    }

    [Fact]
    public async Task AddRoomFailureTest()
    {
        _factory.SetupMoq(true);
        var page = await _browser.NewPageAsync();

        await page.GotoAsync($"{_baseUrl}/RoomControl/AddRoom");

        await page.FillAsync("#Description", "Test Room");
        await page.FillAsync("#Address", "Test Address");
        await page.FillAsync("#PricePerHour", "25.50");

        await page.ClickAsync("button[type=submit]");

        await page.WaitForSelectorAsync("#ErrorMessage");

        Assert.Contains("/AddRoom", page.Url);

        var errorMessage = await page.TextContentAsync("#ErrorMessage");
        Assert.Contains("Ошибка при выполнении запроса", errorMessage);
    }
}