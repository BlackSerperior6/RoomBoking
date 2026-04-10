using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using RoomBooking.Interfaces;

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

        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(12345);

        _host = builder.Build();
        _host.Start();
        
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
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", opts => { });
            
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

    public void SetupMoq(bool setupFailure = false)
    {
        _mockConnectionFactory.Setup(x => x.CreateConnection())
            .Returns(_mockConnection.Object);
        
        _mockConnection.Setup(x => x.CreateCommand(It.IsAny<string>()))
            .Returns(_mockCommand.Object);
        
        if (setupFailure)
            _mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Npgsql.NpgsqlException("Connection failed"));
        else
            _mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        
        _mockCommand.Setup(x => x.AddParameter(
            It.IsAny<string>(),
            It.IsAny<NpgsqlTypes.NpgsqlDbType>(),
            It.IsAny<object>()))
            .Verifiable();
    }
}