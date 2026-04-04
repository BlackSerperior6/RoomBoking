using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using RoomBooking.Interfaces;
using RoomBooking.Wrappers;

namespace RoomBooking.UITests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Mock<IDatabaseConnectionFactory> _mockConnectionFactory = new();
    private readonly Mock<IUserContextWrapper> _mockUserContext = new();
    private readonly Mock<IDbConnectionWrapper> _mockConnection = new();
    private readonly Mock<IDbCommandWrapper> _mockCommand = new();

    public string BaseUrl = "http://localhost:{0}";

    private IHost _host;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        int port = GetRandomUnusedPort();

        BaseUrl = string.Format(BaseUrl, port);

        builder.ConfigureWebHost(webHost =>
        {
            webHost.UseUrls(BaseUrl);
            webHost.UseKestrel();
        });

        _host = builder.Build();
        _host.Start();

        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(12345);
        
        return _host;
    }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbFactoryDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDatabaseConnectionFactory));
            if (dbFactoryDescriptor != null)
                services.Remove(dbFactoryDescriptor);
            
            var userContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IUserContextWrapper));
            if (userContextDescriptor != null)
                services.Remove(userContextDescriptor);
            
            services.AddScoped(_ => _mockConnectionFactory.Object);
            services.AddScoped(_ => _mockUserContext.Object);
        });
        
        builder.UseEnvironment("Development");
    }

    private int GetRandomUnusedPort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void SetupAuthBypass()
    {
        _mockConnectionFactory.Setup(x => x.CreateConnection())
            .Returns(_mockConnection.Object);

        
    }

    public void SetupSuccessfulRoomCreation()
    {
        _mockConnectionFactory.Setup(x => x.CreateConnection())
            .Returns(_mockConnection.Object);
        
        _mockConnection.Setup(x => x.CreateCommand(It.IsAny<string>()))
            .Returns(_mockCommand.Object);
        
        _mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        _mockCommand.Setup(x => x.AddParameter(
            It.IsAny<string>(),
            It.IsAny<NpgsqlTypes.NpgsqlDbType>(),
            It.IsAny<object>()))
            .Verifiable();
    }

    public void SetupFailureRoomCreation()
    {
        _mockConnectionFactory.Setup(x => x.CreateConnection())
            .Returns(_mockConnection.Object);
        
        _mockConnection.Setup(x => x.CreateCommand(It.IsAny<string>()))
            .Returns(_mockCommand.Object);
        
        _mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Npgsql.NpgsqlException("Connection failed"));
        
        _mockCommand.Setup(x => x.AddParameter(
            It.IsAny<string>(),
            It.IsAny<NpgsqlTypes.NpgsqlDbType>(),
            It.IsAny<object>()))
            .Verifiable();
    }
}