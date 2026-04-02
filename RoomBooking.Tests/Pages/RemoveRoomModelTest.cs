using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using RoomBooking.Interfaces;
using RoomBooking.Pages.RoomControl;

namespace RoomBooking.Tests.Pages;

public class RemoveRoomModelTest
{
    private readonly Mock<IDatabaseConnectionFactory> _mockConnectionFactory;

    private readonly Mock<IUserContextWrapper> _mockUserContext;
    private readonly Mock<IDbConnectionWrapper> _mockConnection;
    private readonly Mock<IDbCommandWrapper> _mockCommand;
    private readonly RemoveRoomModel _pageModel;

    public RemoveRoomModelTest()
    {
        _mockConnectionFactory = new Mock<IDatabaseConnectionFactory>();
        _mockUserContext = new Mock<IUserContextWrapper>();
        _mockConnection = new Mock<IDbConnectionWrapper>();
        _mockCommand = new Mock<IDbCommandWrapper>();
        
        _pageModel = new RemoveRoomModel(
            _mockConnectionFactory.Object,
            _mockUserContext.Object);
    }

    [Fact]
    public async Task OnPostAsyncDbSuccess()
    {
        var expectedUserId = 12345L;
        var expectedRoomId = 67;

        _pageModel.RoomId = expectedRoomId;

        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(expectedUserId);

        _pageModel.RoomId = expectedRoomId;

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
                .ReturnsAsync(1);
        
        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);

        Assert.Equal("/Profile", redirectResult.PageName);

        Assert.Equal("Комната была успешно удалена", redirectResult.RouteValues["successMessage"]);

        _mockCommand.Verify(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
            
        _mockConnection.Verify(x => x.DisposeAsync(), Times.Once);
        
        _mockCommand.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task OnPostAsyncNoRoomsFoundTest()
    {
        var expectedUserId = 12345L;
        var expectedRoomId = 67;

        _pageModel.RoomId = expectedRoomId;

        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(expectedUserId);

        _pageModel.RoomId = expectedRoomId;

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
                .ReturnsAsync(0);
        
        var result = await _pageModel.OnPostAsync();

         var redirectResult = Assert.IsType<PageResult>(result);

        Assert.Equal("Не существует комнаты с таким id принадлежащей вам", 
        _pageModel.ErrorMessage);

        _mockCommand.Verify(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
            
        _mockConnection.Verify(x => x.DisposeAsync(), Times.Once);
        
        _mockCommand.Verify(x => x.DisposeAsync(), Times.Once);
    }

     [Fact]
    public async Task OnPostAyncDbErrorTest()
    {
        var expectedUserId = 12345L;
        var expectedRoomId = 67;

        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(expectedUserId);
            
        _pageModel.RoomId = expectedRoomId;
        
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