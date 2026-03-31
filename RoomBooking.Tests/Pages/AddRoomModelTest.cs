using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using NpgsqlTypes;
using RoomBooking.Interfaces;
using RoomBooking.Pages.RoomControl;

namespace RoomBooking.Tests.Pages;

public class AddRoomModelTest
{
    private readonly Mock<IDatabaseConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IUserContextWrapper> _mockUserContext;
    private readonly Mock<IDbConnectionWrapper> _mockConnection;
    private readonly Mock<IDbCommandWrapper> _mockCommand;
    private readonly AddRoomModel _pageModel;

    public AddRoomModelTest()
    {
        _mockConnectionFactory = new Mock<IDatabaseConnectionFactory>();
        _mockUserContext = new Mock<IUserContextWrapper>();
        _mockConnection = new Mock<IDbConnectionWrapper>();
        _mockCommand = new Mock<IDbCommandWrapper>();
        
        _pageModel = new AddRoomModel(
            _mockConnectionFactory.Object,
            _mockUserContext.Object);
    }
    
    [Fact]
    public async Task OnPostAsyncDbSuccess()
    {
        var expectedUserId = 12345L;
        var expectedDescription = "Conference Room";
        var expectedAddress = "123 Business St";
        var expectedPrice = 50.00m;

        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(expectedUserId);

        _pageModel.Description = expectedDescription;
        _pageModel.Address = expectedAddress;
        _pageModel.PricePerHour = expectedPrice;

        _mockConnectionFactory
                .Setup(x => x.CreateConnection())
                .Returns(_mockConnection.Object);
        
        _mockConnection
                .Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        
        _mockConnection
                .Setup(x => x.CreateCommand(It.IsAny<string>()))
                .Returns(_mockCommand.Object);
        
        var addedParameters = new List<(string, NpgsqlDbType, object)>();
        
        _mockCommand
                .Setup(x => x.AddParameter(It.IsAny<string>(), It.IsAny<NpgsqlDbType>(), It.IsAny<object>()))
                .Callback<string, NpgsqlDbType, object>((name, type, value) =>
                {
                    addedParameters.Add((name, type, value));
                });

        _mockCommand
                .Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        
        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);

        Assert.Equal("/Profile", redirectResult.PageName);

        Assert.Equal("Комната была успешно добавлена!", redirectResult.RouteValues["successMessage"]);

        _mockCommand.Verify(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
            
        _mockConnection.Verify(x => x.DisposeAsync(), Times.Once);
        
        _mockCommand.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task OnPostAsyncDbFailure()
    {
        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(12345L);
            
        _pageModel.Description = "Conference Room";
        _pageModel.Address = "123 Business St";
        _pageModel.PricePerHour = 50.00m;
        
        _mockConnectionFactory
        .Setup(x => x.CreateConnection())
        .Returns(_mockConnection.Object);
        
        _mockConnection
        .Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
        
        _mockConnection
        .Setup(x => x.CreateCommand(It.IsAny<string>()))
        .Returns(_mockCommand.Object);
        
        _mockCommand
        .Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Npgsql.NpgsqlException("Connection failed"));
        
        var result = await _pageModel.OnPostAsync();
        
        var pageResult = Assert.IsType<PageResult>(result);
        Assert.Contains("Ошибка при выполнении запроса", _pageModel.ErrorMessage);        
    }
}
