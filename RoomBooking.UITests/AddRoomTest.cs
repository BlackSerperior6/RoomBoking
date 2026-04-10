using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RoomBooking.UITests;

public class AddRoomTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AddRoomTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddRoomValidTest()
    {
        _factory.ResetMocks();
        _factory.SetupMoq();

        var successClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var getResponse = await successClient.GetAsync("/RoomControl/AddRoom");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        
        var token = ExtractAntiForgeryToken(getContent);

        var formData = new Dictionary<string, string>
        {
            ["Description"] = "Test Room",
            ["Address"] = "Test Address",
            ["PricePerHour"] = "50",
            ["__RequestVerificationToken"] = token 
        };

        var response = await successClient.PostAsync("/RoomControl/AddRoom", 
            new FormUrlEncodedContent(formData));
        
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Profile", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AddRoomFailureTest()
    {
        _factory.ResetMocks();
        _factory.SetupMoq(false);

        var failureClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var getResponse = await failureClient.GetAsync("/RoomControl/AddRoom");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        
        var token = ExtractAntiForgeryToken(getContent);
        
        var formData = new Dictionary<string, string>
        {
            ["Description"] = "Test Room",
            ["Address"] = "Test Address",
            ["PricePerHour"] = "25",
            ["__RequestVerificationToken"] = token 
        };
        
        var response = await failureClient.PostAsync("/RoomControl/AddRoom", 
            new FormUrlEncodedContent(formData));
        
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/RoomControl/AddRoom", response.RequestMessage?.RequestUri?.PathAndQuery);
    }

    private string ExtractAntiForgeryToken(string html)
    {
        var pattern = @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""";
        var match = System.Text.RegularExpressions.Regex.Match(html, pattern);
        return match.Success ? match.Groups[1].Value : throw new Exception("Token not found");
    }
}