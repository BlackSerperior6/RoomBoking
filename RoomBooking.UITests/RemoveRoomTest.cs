using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RoomBooking.UITests;

public class RemoveRoomTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    
    public RemoveRoomTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RemoveRoomSuccessTest()
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
            ["RoomId"] = "2",
            ["__RequestVerificationToken"] = token 
        };

        var response = await successClient.PostAsync("/RoomControl/RemoveRoom", 
            new FormUrlEncodedContent(formData));
        
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/Profile", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task RemoveRoomFailureTest()
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
            ["RoomId"] = "2",
            ["__RequestVerificationToken"] = token 
        };
        
        var response = await failureClient.PostAsync("/RoomControl/RemoveRoom", 
            new FormUrlEncodedContent(formData));
        
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/RoomControl/RemoveRoom", response.RequestMessage?.RequestUri?.PathAndQuery);
    }

    private string ExtractAntiForgeryToken(string html)
    {
        var pattern = @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""";
        var match = System.Text.RegularExpressions.Regex.Match(html, pattern);
        return match.Success ? match.Groups[1].Value : throw new Exception("Token not found");
    }
}